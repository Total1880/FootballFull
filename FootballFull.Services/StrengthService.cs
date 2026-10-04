using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;

namespace FootballFull.Services
{
    public class StrengthService : IStrengthService
    {
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubLeagueCompetitionService _clubLeagueCompetitionService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;

        public StrengthService(
            IClubService clubService, 
            ICompetitionService competitionService, 
            IClubLeagueCompetitionService clubLeagueCompetitionService, 
            IClubPerCompetitionService clubPerCompetitionService)
        {
            _clubService = clubService;
            _competitionService = competitionService;
            _clubLeagueCompetitionService = clubLeagueCompetitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
        }
        public void RecalculateClubStrengths()
        {
            var competitions = _competitionService.GetCompetitions().Where(c => c.Type == Competition.CompetitionType.League).ToList();

            var processedClubs = new HashSet<Guid>();
            foreach (var competition in competitions.OrderBy(c => c.Tier))
            {
                var list = _clubLeagueCompetitionService.GetClubLeagueCompetitionsByCompetitionId(competition.Id);
                var ranked = _clubLeagueCompetitionService.GetOrderedRanking(list.ToList()).ToList();
                int teamCount = ranked.Count();

                for (int i = 0; i < teamCount; i++)
                {
                    var entry = ranked[i];
                    if (!processedClubs.Add(entry.ClubId))
                        continue;
                    int position = i + 1;
                    double percentile = position / (double)teamCount;

                    int delta = 0;

                    // Top 20% stijgt
                    if (percentile <= 0.20)
                        delta = +1;
                    // Onderste 20% daalt
                    else if (percentile >= 0.80)
                        delta = -1;

                    var club = _clubService.GetClubById(entry.ClubId);
                    if (club == null)
                        continue;

                    var competitionStrength = competition.Strength;

                    if (delta < 0 && club.Strength + 5 <= competitionStrength)
                        delta = 1;
                    else if (delta > 0 && club.Strength - 5 >= competitionStrength)
                        delta = -1;

                    ClubDevelopment.Apply(club, delta);
                    _clubService.Update(club);
                }
            }
        }

        public void RecalculateCompetitionStrengths(int year)
        {
            // Strength follows the participating clubs; coefficients still determine international rankings.
            foreach (var competition in _competitionService.GetCompetitions()
                .Where(c => c.Type == Competition.CompetitionType.League))
            {
                var clubs = _clubPerCompetitionService.GetClubsForCompetition(competition.Id);
                if (clubs.Count == 0)
                    continue;

                competition.Strength = Math.Clamp(
                    (int)Math.Round(clubs.Average(c => c.Strength), MidpointRounding.AwayFromZero),
                    Configuration.MinStrength, Configuration.MaxStrength);
                _competitionService.Update(competition);
            }
        }
    }
}
