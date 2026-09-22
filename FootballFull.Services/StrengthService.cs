using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Services.Interfaces;
using OlavFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class StrengthService : IStrengthService
    {
        private IClubService _clubService;
        private ICompetitionService _competitionService;
        private IClubLeagueCompetitionService _clubLeagueCompetitionService;
        private IClubInternationalRankingService _clubInternationalRankingService;

        public StrengthService(
            IClubService clubService, 
            ICompetitionService competitionService, 
            IClubLeagueCompetitionService clubLeagueCompetitionService, 
            IClubInternationalRankingService clubInternationalRankingService)
        {
            _clubService = clubService;
            _competitionService = competitionService;
            _clubLeagueCompetitionService = clubLeagueCompetitionService;
            _clubInternationalRankingService = clubInternationalRankingService;
        }
        public void RecalculateClubStrengths()
        {
            var competitions = _competitionService.GetCompetitions().Where(c => c.Type == Competition.CompetitionType.League).ToList();

            foreach (var competition in competitions)
            {
                var list = _clubLeagueCompetitionService.GetClubLeagueCompetitionsByCompetitionId(competition.Id);
                var ranked = _clubLeagueCompetitionService.GetOrderedRanking(list.ToList()).ToList();
                int teamCount = ranked.Count();

                for (int i = 0; i < teamCount; i++)
                {
                    var entry = ranked[i];
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

                    var competitionStrength = _competitionService.GetCompetitions()
                        .First(c => c.Id == entry.CompetitionId).Strength;

                    if (delta < 0 && club.Strength + 3 <= competitionStrength)
                        if (club.Strength + 5 <= competitionStrength)
                            delta = 1;

                    if (delta > 0 && club.Strength - 3 >= competitionStrength)
                        if (club.Strength - 5 >= competitionStrength)
                            delta = -1;

                    var newStrength = club.Strength + ApplyFinancialModifier(club, delta);
                    if (newStrength < Configuration.MinStrength) newStrength = Configuration.MinStrength;
                    if (newStrength > Configuration.MaxStrength) newStrength = Configuration.MaxStrength;

                    club.Strength = newStrength;
                    _clubService.Update(club);
                }
            }
        }

        public void RecalculateCompetitionStrengths(int year)
        {
            var initial = Configuration.MaxStrength - Configuration.MinStrength;
            var countryRankings = _clubInternationalRankingService.GetAll()
.GroupBy(c => c.CountryId)
.Select(g => new
{
    CountryId = g.Key,
    PointsPerClub = g.Average(c => c.TotalPoints(year)) // = totaal / aantal clubs
})
.OrderByDescending(x => x.PointsPerClub)
.ToList();

            var competitions = _competitionService.GetCompetitions().Where(_ => _.Type == Competition.CompetitionType.League);
            foreach (var country in countryRankings)
            {
                var current = initial;
                var competitionsCountry = competitions.Where(_ => _.CountryId == country.CountryId).OrderBy(_ => _.Tier).ToList();
                var step = current / (competitionsCountry.Count == 0 ? 1 : competitionsCountry.Count);

                foreach (var competition in competitionsCountry)
                {
                    competition.Strength = current;
                    _competitionService.Update(competition);
                    current -= step;

                }
                initial--;
            }
        }

        private int ApplyFinancialModifier(Club club, int sportingDelta)
        {
            if (club.Balance < 0 && sportingDelta > 0)
                return 0;

            if (club.Balance < -100_000)
                return -1;

            return sportingDelta;
        }
    }
}
