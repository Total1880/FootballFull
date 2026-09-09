using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System.Data;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionRulesService : ICompetitionRulesService
    {
        private ICompetitionService _competitionService;
        private IClubPerCompetitionService _clubPerCompetitionService;
        private IClubLeagueCompetitionService _clubLeagueCompetitionService;
        private IClubService _clubService;
        private IRepository<CompetitionRules> _repository;

        public CompetitionRulesService(
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            IClubLeagueCompetitionService clubLeagueCompetitionService,
            IClubService clubService,
            IRepository<CompetitionRules> repository)
        {
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _clubLeagueCompetitionService = clubLeagueCompetitionService;
            _clubService = clubService;
            _repository = repository;
        }

        public CompetitionRules GetCompetitionRules(Guid competitionId)
        {
            var rules = _repository.Load().FirstOrDefault(_ => _.CompetitionId == competitionId);
            if (rules != null)
            {
                rules.Competition = _competitionService.GetCompetitionById(rules.CompetitionId);
                rules.PromotionTo = _competitionService.GetCompetitionById(rules.CompetitionPromotionToId);
                rules.RelegationTo = _competitionService.GetCompetitionById(rules.CompetitionRelegationToId);
            }
            return rules == null ? new CompetitionRules { CompetitionId = competitionId } : rules;
        }

        public void ApplyPromotionAndRelegations(IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            var allLeagueCompetitions = _competitionService.GetCompetitions().Where(_ => _.Type == CompetitionType.League);
            var allClubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            var allClubs = _clubService.GetClubs();
            var regularMoves = new List<ClubMove>();
            var extraRelegations = new Dictionary<Guid, Guid>(); // Key = ClubId, Value = CompetitionId

            foreach (var competition in allLeagueCompetitions)
            {
                var clubLeagueCompetition = _clubLeagueCompetitionService.GetOrderedRanking(clubLeagueCompetitions.Where(_ => _.CompetitionId.Equals(competition.Id)).ToList()).ToList();

                var rules = GetCompetitionRules(competition.Id);
                if (rules == null)
                    continue;

                ApplyPromotions(competition, rules, regularMoves, clubLeagueCompetition, allClubs, allClubsPerCompetition);
                ApplyRelegations(competition, rules, regularMoves, clubLeagueCompetition, allClubs, allClubsPerCompetition, extraRelegations);
            }

            ApplyMoves(regularMoves, allClubsPerCompetition);
        }

        private void ApplyMoves(List<ClubMove> moves, IList<ClubPerCompetition> allClubsPerCompetition)
        {
            foreach (var move in moves.Where(m => m.Type == ClubMoveType.Remove))
            {
                _clubPerCompetitionService.RemoveClubFromCompetition(move.ClubId, move.CompetitionId);
            }

            foreach (var move in moves.Where(m => m.Type == ClubMoveType.Add))
            {
                _clubPerCompetitionService.AddClubToCompetition(move.ClubId, move.CompetitionId);
            }
        }

        private void ApplyPromotions(Competition competition, CompetitionRules rules, List<ClubMove> moves, List<ClubLeagueCompetition> clubLeagueCompetition, IList<Club> allClubs, IList<ClubPerCompetition> allClubsPerCompetition)
        {
            if (rules.PromotionTo == null || rules.PromotionPlaces <= 0)
                return;

            var promoted = 0;
            var index = 0;

            while (promoted < rules.PromotionPlaces && index < clubLeagueCompetition.Count)
            {
                var clubId = clubLeagueCompetition[index].ClubId;
                index++;

                if (PromotionBlockedByParentClub(clubId, allClubsPerCompetition.Where(_ => _.CompetitionId == rules.PromotionTo.Id).ToList(), allClubs.FirstOrDefault(_ => _.FeederClubId == clubId), moves, rules.PromotionTo.Id))
                    continue;

                var club = allClubs.First(_ => _.Id == clubId);

                moves.Add(ClubMove.Remove(club.Id, competition.Id));
                moves.Add(ClubMove.Add(club.Id, rules.PromotionTo.Id));

                promoted++;
            }
        }

        private void ApplyRelegations(Competition competition, CompetitionRules rules, List<ClubMove> regularMoves, List<ClubLeagueCompetition> clubLeagueCompetition, IList<Club> allClubs, IList<ClubPerCompetition> allClubsPerCompetition, Dictionary<Guid, Guid> extraRelegations)
        {
            if (rules.RelegationTo == null || rules.RelegationPlaces <= 0)
                return;

            var relegated = extraRelegations.Where(_ => _.Value == competition.Id).ToList().Count();
            var indexFromBottom = 0;
            var subCompetitionCounter = 0;

            while (relegated < rules.RelegationPlaces && indexFromBottom < clubLeagueCompetition.Count)
            {
                var rankingIndex = clubLeagueCompetition.Count - 1 - indexFromBottom;
                var clubToRelegateId = clubLeagueCompetition[rankingIndex].ClubId;

                if (regularMoves.Any(_ => _.ClubId == clubToRelegateId))
                    continue;

                indexFromBottom++;

                var clubToRelegate = allClubs.First(_ => _.Id == clubToRelegateId);

                if (clubToRelegate == null)
                    continue;

                var conflictWasHandled = TryHandleFeederClubConflict(
                    competition,
                    rules,
                    clubToRelegate,
                    ref subCompetitionCounter,
                    regularMoves,
                    allClubs,
                    clubLeagueCompetition,
                    allClubsPerCompetition,
                    extraRelegations);

                if (!conflictWasHandled)
                {
                    MoveClubToCompetition(
                        clubToRelegate,
                        competition,
                        rules.RelegationTo,
                        ref subCompetitionCounter,
                        regularMoves);

                    relegated++;
                }
            }
        }

        private bool TryHandleFeederClubConflict(
    Competition currentCompetition,
    CompetitionRules currentRules,
    Club clubToRelegate,
    ref int subCompetitionCounter,
    List<ClubMove> moves,
    IList<Club> allClubs,
    List<ClubLeagueCompetition> clubLeagueCompetition,
    IList<ClubPerCompetition> allClubsPerCompetition,
    Dictionary<Guid, Guid> extraRelegations)
        {
            if (clubToRelegate.FeederClubId == null)
                return false;

            var feederClub = allClubs.First(_ => _.Id == clubToRelegate.FeederClubId.Value);

            if (feederClub == null)
                return false;

            var relegationTarget = currentRules.RelegationTo;

            if (relegationTarget == null)
                return false;

            var feederClubIsAlreadyInTarget = false;
            if (relegationTarget.Type == CompetitionType.ParentCompetition)
            {
                for (int i = 0; i < relegationTarget.SubCompetitionIds.Count; i++)
                {
                    feederClubIsAlreadyInTarget = CompetitionContainsClub(allClubsPerCompetition.Where(_ => _.CompetitionId == relegationTarget.SubCompetitionIds[i]).ToList(), feederClub.Id);
                    if (feederClubIsAlreadyInTarget) break;
                }
            }
            else
            {
                feederClubIsAlreadyInTarget = CompetitionContainsClub(
                allClubsPerCompetition.Where(_ => _.CompetitionId == relegationTarget.Id).ToList(),
                feederClub.Id);
            }



            if (!feederClubIsAlreadyInTarget)
                return false;

            var lowerRules = GetCompetitionRules(relegationTarget.Id);

            if (lowerRules == null || lowerRules.RelegationTo == null)
            {
                // Feeder zit al in de lagere reeks en kan zelf niet lager.
                // Dan slaan we deze degradatie inhoudelijk over.
                return true;
            }

            MoveClubToCompetition(
                feederClub,
                relegationTarget,
                lowerRules.RelegationTo,
                ref subCompetitionCounter,
                moves);

            extraRelegations.Add(feederClub.Id, relegationTarget.Id);

            return false;
        }

        private void MoveClubToCompetition(
    Club club,
    Competition fromCompetition,
    Competition toCompetition,
    ref int subCompetitionCounter,
    List<ClubMove> moves)
        {
            var targetCompetition = ResolveTargetCompetition(
                club,
                toCompetition,
                subCompetitionCounter);

            if (targetCompetition == null)
                return;

            if (toCompetition.SubCompetitionIds != null && toCompetition.SubCompetitionIds.Count > 0)
            {
                var availableSubCompetitions = GetMatchingSubCompetitions(club, toCompetition);

                if (availableSubCompetitions.Count > 0)
                {
                    subCompetitionCounter++;

                    if (subCompetitionCounter >= availableSubCompetitions.Count)
                        subCompetitionCounter = 0;
                }
            }

            moves.Add(ClubMove.Remove(club.Id, fromCompetition.Id));
            moves.Add(ClubMove.Add(club.Id, targetCompetition.Id));
        }

        private Competition ResolveTargetCompetition(
    Club club,
    Competition competition,
    int subCompetitionCounter)
        {
            if (competition.SubCompetitionIds == null || competition.SubCompetitionIds.Count == 0)
                return competition;

            var availableSubCompetitions = GetMatchingSubCompetitions(club, competition);

            if (!availableSubCompetitions.Any())
                return null;

            if (subCompetitionCounter >= availableSubCompetitions.Count)
                subCompetitionCounter = 0;

            return availableSubCompetitions[subCompetitionCounter];
        }

        private List<Competition> GetMatchingSubCompetitions(
    Club club,
    Competition competition)
        {
            if (competition.SubCompetitionIds == null)
                return new List<Competition>();

            var subCompetitions = _competitionService.GetSubCompetitions(competition);

            var matchingSubCompetitions = subCompetitions
                .Where(subCompetition =>
                    subCompetition.SplitParameters != null &&
                    club.CompetitionSplitParameters != null &&
                    subCompetition.SplitParameters.Any(subParameter =>
                        club.CompetitionSplitParameters.Any(clubParameter =>
                            clubParameter.Id == subParameter.Id)))
                .ToList();

            return matchingSubCompetitions;
        }

        private bool PromotionBlockedByParentClub(Guid clubId, List<ClubPerCompetition> clubPerCompetition, Club? parentClub, List<ClubMove> moves, Guid promotionCompetitionId)
        {
            if (moves.Any(_ => _.ClubId == clubId)) // If the club is already moved, we don't need to check for parent club conflicts
                return true;


            var parentClubIsInTargetCompetition = parentClub != null && CompetitionContainsClub(clubPerCompetition, parentClub.Id);
            if (!parentClubIsInTargetCompetition && moves.Any(_ => _.Type == ClubMoveType.Add && _.ClubId == parentClub?.Id && _.CompetitionId == promotionCompetitionId))
            {
                parentClubIsInTargetCompetition = true;
            }
            else if (parentClubIsInTargetCompetition && moves.Any(_ => _.Type == ClubMoveType.Remove && _.ClubId == parentClub?.Id && _.CompetitionId == promotionCompetitionId))
            {
                parentClubIsInTargetCompetition = false;
            }



            return parentClubIsInTargetCompetition;
        }

        private bool CompetitionContainsClub(IList<ClubPerCompetition> clubPerCompetition, Guid id)
        {
            return clubPerCompetition.Any(_ => _.ClubId == id);
        }

        private void AddCompetitionRecursive(
    List<Competition> result,
    Competition competition)
        {
            if (competition == null)
                return;

            if (!result.Any(c => c.Id == competition.Id))
                result.Add(competition);

            if (competition.SubCompetitions == null)
                return;

            foreach (var subCompetition in competition.SubCompetitions)
            {
                AddCompetitionRecursive(result, subCompetition);
            }
        }

        public bool Save(CompetitionRules competitionRules)
        {
            if (competitionRules.Id == Guid.Empty)
            {
                competitionRules.Id = new Guid();
                _repository.Add(competitionRules);
            }
            else
                _repository.Update(competitionRules);

            return true;
        }

        private class ClubMove
        {
            public Guid ClubId { get; private set; }
            public Guid CompetitionId { get; private set; }
            public ClubMoveType Type { get; private set; }

            public static ClubMove Add(Guid clubId, Guid competitionId)
            {
                return new ClubMove
                {
                    ClubId = clubId,
                    CompetitionId = competitionId,
                    Type = ClubMoveType.Add
                };
            }

            public static ClubMove Remove(Guid clubId, Guid competitionId)
            {
                return new ClubMove
                {
                    ClubId = clubId,
                    CompetitionId = competitionId,
                    Type = ClubMoveType.Remove
                };
            }
        }

        private enum ClubMoveType
        {
            Add,
            Remove
        }
    }
}
