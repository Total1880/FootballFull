using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionRulesService : ICompetitionRulesService
    {
        private readonly ICompetitionService _competitionService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly IClubLeagueCompetitionService _clubLeagueCompetitionService;
        private readonly IClubService _clubService;
        private readonly IRepository<CompetitionRules> _repository;

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
            var rules = _repository.Load()
                .FirstOrDefault(rule => rule.CompetitionId == competitionId);

            if (rules == null)
                return new CompetitionRules { CompetitionId = competitionId };

            rules.Competition = _competitionService.GetCompetitionById(rules.CompetitionId);
            rules.PromotionTo = GetCompetitionOrNull(rules.CompetitionPromotionToId);
            rules.RelegationTo = GetCompetitionOrNull(rules.CompetitionRelegationToId);

            return rules;
        }

        public void ApplyPromotionAndRelegations(
            IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            var competitions = _competitionService.GetCompetitions()
                .Where(competition => competition.Type == CompetitionType.League)
                .OrderBy(competition => competition.Tier)
                .ThenBy(competition => competition.Name)
                .ToList();

            var allClubs = _clubService.GetClubs().ToList();
            var allClubsPerCompetition = _clubPerCompetitionService
                .GetAllClubPerCompetitions()
                .ToList();

            var rankings = competitions.ToDictionary(
                competition => competition.Id,
                competition => _clubLeagueCompetitionService
                    .GetOrderedRanking(clubLeagueCompetitions
                        .Where(row => row.CompetitionId == competition.Id)
                        .ToList())
                    .ToList());

            var rulesByCompetition = competitions.ToDictionary(
                competition => competition.Id,
                competition => GetCompetitionRules(competition.Id));

            var state = new MovementState();

            // Tier per tier is essential: when a lower tier is processed, all
            // relegations received from the tier above are already known.
            foreach (var tierGroup in competitions.GroupBy(c => c.Tier).OrderBy(g => g.Key))
            {
                foreach (var competition in tierGroup)
                {
                    var rules = rulesByCompetition[competition.Id];
                    var ranking = rankings[competition.Id];

                    ApplyPromotions(
                        competition,
                        rules,
                        ranking,
                        allClubs,
                        allClubsPerCompetition,
                        state);
                }

                foreach (var competition in tierGroup)
                {
                    var rules = rulesByCompetition[competition.Id];
                    var ranking = rankings[competition.Id];

                    var received = GetCount(state.RelegationsReceived, competition.Id);
                    var promoted = GetCount(state.PromotionsOut, competition.Id);
                    var balance = received - promoted;

                    if (balance < 0)
                    {
                        // This competition lost more clubs through promotion than
                        // it received through relegation. A lower competition must
                        // therefore supply additional promoted clubs.
                        AddCount(state.PromotionBonuses, competition.Id, -balance);
                    }

                    var extraRelegations = Math.Max(0, balance);
                    var forcedRelegations = GetCount(
                        state.ForcedRelegationsOut,
                        competition.Id);

                    var relegationsToSelect = Math.Max(
                        0,
                        rules.RelegationPlaces + extraRelegations - forcedRelegations);

                    ApplyRelegations(
                        competition,
                        rules,
                        ranking,
                        relegationsToSelect,
                        allClubs,
                        allClubsPerCompetition,
                        state);
                }
            }

            ApplyMoves(state.Moves);
        }

        private void ApplyPromotions(
            Competition competition,
            CompetitionRules rules,
            IList<ClubLeagueCompetition> ranking,
            IList<Club> allClubs,
            IList<ClubPerCompetition> allClubsPerCompetition,
            MovementState state)
        {
            if (rules.PromotionTo == null)
                return;

            var promoted = 0;

            // First apply the normal promotion places.
            foreach (var rankingRow in ranking)
            {
                if (promoted >= rules.PromotionPlaces)
                    break;

                var club = allClubs.FirstOrDefault(c => c.Id == rankingRow.ClubId);
                if (club == null || ClubAlreadyMoved(club.Id, state.Moves))
                    continue;

                var target = ResolveTargetCompetition(club, rules.PromotionTo, 0);
                if (target == null || PromotionBlockedByParentClub(
                        club,
                        target,
                        allClubs,
                        allClubsPerCompetition,
                        state.Moves))
                    continue;

                AddMove(club, competition, target, ClubMoveReason.Promotion, state);
                promoted++;
            }

            // Then consume bonuses meant for this exact competition or for one
            // of the subcompetitions of the configured parent competition.
            foreach (var bonusTarget in GetBonusTargets(rules.PromotionTo, state).ToList())
            {
                while (GetCount(state.PromotionBonuses, bonusTarget.Id) > 0)
                {
                    var club = FindNextPromotableClub(
                        ranking,
                        bonusTarget,
                        allClubs,
                        allClubsPerCompetition,
                        state.Moves);

                    if (club == null)
                        break;

                    AddMove(
                        club,
                        competition,
                        bonusTarget,
                        ClubMoveReason.Promotion,
                        state);

                    AddCount(state.PromotionBonuses, bonusTarget.Id, -1);
                }
            }
        }

        private void ApplyRelegations(
            Competition competition,
            CompetitionRules rules,
            IList<ClubLeagueCompetition> ranking,
            int numberOfRelegations,
            IList<Club> allClubs,
            IList<ClubPerCompetition> allClubsPerCompetition,
            MovementState state)
        {
            if (rules.RelegationTo == null || numberOfRelegations <= 0)
                return;

            var relegated = 0;
            var subCompetitionCounter = 0;

            for (var index = ranking.Count - 1;
                 index >= 0 && relegated < numberOfRelegations;
                 index--)
            {
                var club = allClubs.FirstOrDefault(c => c.Id == ranking[index].ClubId);
                if (club == null || ClubAlreadyMoved(club.Id, state.Moves))
                    continue;

                var target = ResolveTargetCompetition(
                    club,
                    rules.RelegationTo,
                    subCompetitionCounter);

                if (target == null)
                    continue;

                if (!TryMakeRoomForParentClub(
                        club,
                        target,
                        allClubs,
                        allClubsPerCompetition,
                        state))
                    continue;

                AddMove(
                    club,
                    competition,
                    target,
                    ClubMoveReason.Relegation,
                    state);

                relegated++;

                if (rules.RelegationTo.SubCompetitionIds.Count > 0)
                    subCompetitionCounter++;
            }
        }

        private bool TryMakeRoomForParentClub(
            Club parentClub,
            Competition relegationTarget,
            IList<Club> allClubs,
            IList<ClubPerCompetition> allClubsPerCompetition,
            MovementState state)
        {
            if (parentClub.FeederClubId == null)
                return true;

            var feederClub = allClubs.FirstOrDefault(
                club => club.Id == parentClub.FeederClubId.Value);

            if (feederClub == null || !ClubWillBeInCompetition(
                    feederClub.Id,
                    relegationTarget.Id,
                    allClubsPerCompetition,
                    state.Moves))
                return true;

            var feederRules = GetCompetitionRules(relegationTarget.Id);
            if (feederRules.RelegationTo == null)
                return false;

            var feederTarget = ResolveTargetCompetition(
                feederClub,
                feederRules.RelegationTo,
                0);

            if (feederTarget == null || ClubAlreadyMoved(feederClub.Id, state.Moves))
                return false;

            AddMove(
                feederClub,
                relegationTarget,
                feederTarget,
                ClubMoveReason.ForcedRelegation,
                state);

            return true;
        }

        private void AddMove(
            Club club,
            Competition from,
            Competition to,
            ClubMoveReason reason,
            MovementState state)
        {
            state.Moves.Add(ClubMove.Remove(club.Id, from.Id, reason));
            state.Moves.Add(ClubMove.Add(club.Id, to.Id, reason));

            if (reason == ClubMoveReason.Promotion)
            {
                AddCount(state.PromotionsOut, from.Id, 1);
                return;
            }

            AddCount(state.RelegationsReceived, to.Id, 1);

            if (reason == ClubMoveReason.ForcedRelegation)
                AddCount(state.ForcedRelegationsOut, from.Id, 1);
        }

        private Club? FindNextPromotableClub(
            IEnumerable<ClubLeagueCompetition> ranking,
            Competition requiredTarget,
            IList<Club> allClubs,
            IList<ClubPerCompetition> allClubsPerCompetition,
            IList<ClubMove> moves)
        {
            foreach (var rankingRow in ranking)
            {
                var club = allClubs.FirstOrDefault(c => c.Id == rankingRow.ClubId);
                if (club == null || ClubAlreadyMoved(club.Id, moves))
                    continue;

                if (!ClubMatchesCompetition(club, requiredTarget))
                    continue;

                if (PromotionBlockedByParentClub(
                        club,
                        requiredTarget,
                        allClubs,
                        allClubsPerCompetition,
                        moves))
                    continue;

                return club;
            }

            return null;
        }

        private IEnumerable<Competition> GetBonusTargets(
            Competition promotionTarget,
            MovementState state)
        {
            if (promotionTarget.SubCompetitionIds.Count == 0)
            {
                if (GetCount(state.PromotionBonuses, promotionTarget.Id) > 0)
                    yield return promotionTarget;

                yield break;
            }

            foreach (var subCompetition in _competitionService
                         .GetSubCompetitions(promotionTarget))
            {
                if (GetCount(state.PromotionBonuses, subCompetition.Id) > 0)
                    yield return subCompetition;
            }
        }

        private Competition? ResolveTargetCompetition(
            Club club,
            Competition competition,
            int subCompetitionCounter)
        {
            if (competition.SubCompetitionIds.Count == 0)
                return competition;

            var matches = GetMatchingSubCompetitions(club, competition);
            if (matches.Count == 0)
                return null;

            return matches[subCompetitionCounter % matches.Count];
        }

        private List<Competition> GetMatchingSubCompetitions(
            Club club,
            Competition competition)
        {
            if (competition.SubCompetitionIds.Count == 0)
                return new List<Competition>();

            return _competitionService.GetSubCompetitions(competition)
                .Where(subCompetition => ClubMatchesCompetition(club, subCompetition))
                .ToList();
        }

        private static bool ClubMatchesCompetition(
            Club club,
            Competition competition)
        {
            if (competition.SplitParameters.Count == 0)
                return true;

            return competition.SplitParameters.Any(subParameter =>
                club.CompetitionSplitParameters.Any(clubParameter =>
                    clubParameter.Id == subParameter.Id));
        }

        private static bool PromotionBlockedByParentClub(
            Club club,
            Competition target,
            IList<Club> allClubs,
            IList<ClubPerCompetition> allClubsPerCompetition,
            IList<ClubMove> moves)
        {
            var parentClub = allClubs.FirstOrDefault(
                possibleParent => possibleParent.FeederClubId == club.Id);

            return parentClub != null && ClubWillBeInCompetition(
                parentClub.Id,
                target.Id,
                allClubsPerCompetition,
                moves);
        }

        private static bool ClubWillBeInCompetition(
            Guid clubId,
            Guid competitionId,
            IList<ClubPerCompetition> currentMemberships,
            IList<ClubMove> moves)
        {
            var isInCompetition = currentMemberships.Any(membership =>
                membership.ClubId == clubId &&
                membership.CompetitionId == competitionId);

            if (moves.Any(move =>
                    move.Type == ClubMoveType.Remove &&
                    move.ClubId == clubId &&
                    move.CompetitionId == competitionId))
                isInCompetition = false;

            if (moves.Any(move =>
                    move.Type == ClubMoveType.Add &&
                    move.ClubId == clubId &&
                    move.CompetitionId == competitionId))
                isInCompetition = true;

            return isInCompetition;
        }

        private static bool ClubAlreadyMoved(
            Guid clubId,
            IEnumerable<ClubMove> moves)
        {
            return moves.Any(move => move.ClubId == clubId);
        }

        private Competition? GetCompetitionOrNull(Guid competitionId)
        {
            return competitionId == Guid.Empty
                ? null
                : _competitionService.GetCompetitionById(competitionId);
        }

        private void ApplyMoves(IEnumerable<ClubMove> moves)
        {
            foreach (var move in moves.Where(move => move.Type == ClubMoveType.Remove))
                _clubPerCompetitionService.RemoveClubFromCompetition(
                    move.ClubId,
                    move.CompetitionId);

            foreach (var move in moves.Where(move => move.Type == ClubMoveType.Add))
                _clubPerCompetitionService.AddClubToCompetition(
                    move.ClubId,
                    move.CompetitionId);
        }

        private static int GetCount(
            IReadOnlyDictionary<Guid, int> values,
            Guid competitionId)
        {
            return values.TryGetValue(competitionId, out var value) ? value : 0;
        }

        private static void AddCount(
            IDictionary<Guid, int> values,
            Guid competitionId,
            int amount)
        {
            values[competitionId] = values.TryGetValue(competitionId, out var current)
                ? current + amount
                : amount;
        }

        public bool Save(CompetitionRules competitionRules)
        {
            if (competitionRules.Id == Guid.Empty)
            {
                competitionRules.Id = Guid.NewGuid();
                _repository.Add(competitionRules);
            }
            else
            {
                _repository.Update(competitionRules);
            }

            return true;
        }

        private sealed class MovementState
        {
            public List<ClubMove> Moves { get; } = new();
            public Dictionary<Guid, int> PromotionsOut { get; } = new();
            public Dictionary<Guid, int> RelegationsReceived { get; } = new();
            public Dictionary<Guid, int> ForcedRelegationsOut { get; } = new();
            public Dictionary<Guid, int> PromotionBonuses { get; } = new();
        }

        private sealed class ClubMove
        {
            public Guid ClubId { get; private init; }
            public Guid CompetitionId { get; private init; }
            public ClubMoveType Type { get; private init; }
            public ClubMoveReason Reason { get; private init; }

            public static ClubMove Add(
                Guid clubId,
                Guid competitionId,
                ClubMoveReason reason)
            {
                return new ClubMove
                {
                    ClubId = clubId,
                    CompetitionId = competitionId,
                    Type = ClubMoveType.Add,
                    Reason = reason
                };
            }

            public static ClubMove Remove(
                Guid clubId,
                Guid competitionId,
                ClubMoveReason reason)
            {
                return new ClubMove
                {
                    ClubId = clubId,
                    CompetitionId = competitionId,
                    Type = ClubMoveType.Remove,
                    Reason = reason
                };
            }
        }

        private enum ClubMoveType
        {
            Add,
            Remove
        }

        private enum ClubMoveReason
        {
            Promotion,
            Relegation,
            ForcedRelegation
        }
    }
}
