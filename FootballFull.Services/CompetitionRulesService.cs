using FootballFull.Models;
using FootballFull.Services.Interfaces;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionRulesService : ICompetitionRulesService
    {
        private ICompetitionService _competitionService;
        private IClubPerCompetitionService _clubPerCompetitionService;

        public CompetitionRulesService(ICompetitionService competitionService, IClubPerCompetitionService clubPerCompetitionService)
        {
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
        }
        public void ApplyPromotionAndRelegations()
        {
            var allLeagueCompetitions = _competitionService.GetCompetitions().Where(_ => _.Type == CompetitionType.League);
            var allClubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            var regularMoves = new List<ClubMove>();

            foreach (var competition in allLeagueCompetitions)
            {
                var clubIds = allClubsPerCompetition.Where(_ => _.CompetitionId == competition.Id).Select(_ => _.ClubId);
                var rules = GetCompetitionRules(competition.Id);
                if (rules == null)
                    continue;

                ApplyPromotions(competition, rules, regularMoves);
                ApplyRelegations(competition, rules, regularMoves);
            }
        }

        private void ApplyPromotions(Competition competition, CompetitionRules rules, List<ClubMove> regularMoves)
        {
            if (rules.PromotionTo == null || rules.PromotionPlaces <= 0)
                return;
        }

        private void ApplyRelegations(Competition competition, CompetitionRules rules, List<ClubMove> regularMoves)
        {
            throw new NotImplementedException();
        }



        public CompetitionRules GetCompetitionRules(Guid id)
        {
            throw new NotImplementedException();
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
