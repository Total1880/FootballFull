using FootballFull.Models;
using FootballFull.Services.Interfaces;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionRulesService : ICompetitionRulesService
    {
        private ICompetitionService _competitionService;
        private IClubPerCompetitionService _clubPerCompetitionService;
        private IClubLeagueCompetitionService _clubLeagueCompetitionService;
        private IClubService _clubService;

        public CompetitionRulesService(
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            IClubLeagueCompetitionService clubLeagueCompetitionService,
            IClubService clubService)
        {
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _clubLeagueCompetitionService = clubLeagueCompetitionService;
            _clubService = clubService;
        }

        public CompetitionRules GetCompetitionRules(Guid id)
        {
            throw new NotImplementedException();
        }

        public void ApplyPromotionAndRelegations(IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            var allLeagueCompetitions = _competitionService.GetCompetitions().Where(_ => _.Type == CompetitionType.League);
            var allClubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            var allClubs = _clubService.GetClubs();
            var regularMoves = new List<ClubMove>();

            foreach (var competition in allLeagueCompetitions)
            {
                var clubLeagueCompetition = _clubLeagueCompetitionService.GetOrderedRanking(clubLeagueCompetitions.Where(_ => _.CompetitionId.Equals(competition.Id)).ToList()).ToList();

                var rules = GetCompetitionRules(competition.Id);
                if (rules == null)
                    continue;

                ApplyPromotions(competition, rules, regularMoves, clubLeagueCompetition, allClubs);
                ApplyRelegations(competition, rules, regularMoves, clubLeagueCompetition, allClubs);
            }
        }

        private void ApplyPromotions(Competition competition, CompetitionRules rules, List<ClubMove> moves, List<ClubLeagueCompetition> clubLeagueCompetition, IList<Club> allClubs)
        {
            if (rules.PromotionTo == null || rules.PromotionPlaces <= 0)
                return;

            var promoted = 0;
            var index = 0;

            while (promoted < rules.PromotionPlaces && index < clubLeagueCompetition.Count)
            {
                var clubId = clubLeagueCompetition[index].ClubId;
                index++;

                if (PromotionBlockedByParentClub(clubId, clubLeagueCompetition.Where(_ => _.CompetitionId == rules.PromotionTo.Id).ToList(), allClubs.FirstOrDefault(_ => _.FeederClubId == clubId)))
                    continue;

                var club = allClubs.First(_ => _.Id == clubId);

                moves.Add(ClubMove.Remove(club.Id, competition.Id));
                moves.Add(ClubMove.Add(club.Id, rules.PromotionTo.Id));

                promoted++;
            }
        }

        private void ApplyRelegations(Competition competition, CompetitionRules rules, List<ClubMove> regularMoves, IEnumerable<ClubLeagueCompetition> clubLeagueCompetition, IList<Club> allClubs)
        {
            throw new NotImplementedException();
        }

        private bool PromotionBlockedByParentClub(Guid clubId, List<ClubLeagueCompetition> clubLeagueCompetition, Club? parentClub)
        {
            return parentClub != null &&
                   CompetitionContainsClub(clubLeagueCompetition, parentClub.Id);
        }

        private bool CompetitionContainsClub(List<ClubLeagueCompetition> clubLeagueCompetition, Guid id)
        {
            return clubLeagueCompetition.Any(_ => _.ClubId == id);
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
