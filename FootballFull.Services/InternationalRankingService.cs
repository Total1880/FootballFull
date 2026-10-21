using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using OlavFramework;

namespace FootballFull.Services
{
    public class InternationalRankingService : IInternationalRankingService
    {
        private ClubInternationalRanking _ranking;
        private readonly IRepository<ClubInternationalRanking> _repository;
        private readonly IInternationalCompetitionSettingsService _internationalCompetitionSettingsService;
        private readonly IClubService _clubService;
        private readonly ICountryService _countryService;

        public InternationalRankingService(
            IRepository<ClubInternationalRanking> repository,
            IInternationalCompetitionSettingsService internationalCompetitionSettingsService,
            IClubService clubService,
            ICountryService countryService)
        {
            _repository = repository;
            _internationalCompetitionSettingsService = internationalCompetitionSettingsService;
            _clubService = clubService;
            _countryService = countryService;

            _ranking = _repository.Load().FirstOrDefault();
            if (_ranking == null)
                _ranking = new ClubInternationalRanking();
        }
        public ClubInternationalRanking GetAll()
        {
            return _ranking;
        }

        public void SaveAll()
        {
            _repository.Create(new List<ClubInternationalRanking> { _ranking }, true);
        }

        public bool ProcessFixture(Fixture fixture, int year)
        {
            var settings = _internationalCompetitionSettingsService.GetSettings(fixture.CompetitionId);
            if (settings == null)
                throw new InvalidOperationException("International competition settings not found.");

            if (fixture.HomeScore == fixture.AwayScore)
               throw new NotImplementedException("Draws are not supported in international competitions.");

            var winner = fixture.HomeScore > fixture.AwayScore ? fixture.HomeTeamId : fixture.AwayTeamId;
            var loser = fixture.HomeScore > fixture.AwayScore ? fixture.AwayTeamId : fixture.HomeTeamId;
            if (_ranking.Rows.Any(_ => _.YearDetails.Any(_ => _.ClubId == winner && _.Year == year)))
            {
                _ranking.Rows.First(_ => _.YearDetails.Any(_ => _.ClubId == winner && _.Year == year)).YearDetails.First(_ => _.ClubId == winner && _.Year == year).Points += settings.PointsForWin;
            }
            else
            {
                var newRow = new ClubInternationalRankingRow
                {
                    YearDetails = new List<ClubInternationalRankingRowYearDetail>
                    {
                        new ClubInternationalRankingRowYearDetail
                        {
                            ClubId = winner,
                            CountryId = _clubService.GetCountryId(winner),
                            Year = year,
                            Points = settings.PointsForWin
                        }
                    }
                };
                _ranking.Rows.Add(newRow);
            }

            if(!_ranking.Rows.Any(_ => _.YearDetails.Any(_ => _.ClubId == loser && _.Year == year)))
            {
                var newRow = new ClubInternationalRankingRow
                {
                    YearDetails = new List<ClubInternationalRankingRowYearDetail>
                    {
                        new ClubInternationalRankingRowYearDetail
                        {
                            ClubId = loser,
                            CountryId = _clubService.GetCountryId(loser),
                            Year = year,
                            Points = 0
                        }
                    }
                };
                _ranking.Rows.Add(newRow);
            }

            return true;
        }

        public decimal GetCountryPointsForXYears(Guid countryId, int currentYear, int years)
        {
            return GetRanking(currentYear, years).Rows
                .FirstOrDefault(r => r.CountryId == countryId)?.Points ?? 0m;
        }

        public CountryInternationalCompetitionRanking GetRanking(int currentYear, int years)
        {
            var ranking = new CountryInternationalCompetitionRanking();

            if (_ranking == null || years <= 0)
                return ranking;

            var countries = _countryService.GetCountries();

            var allDetails = _ranking.Rows
                .SelectMany(r => r.YearDetails)
                .ToList();

            int startYear = currentYear - years + 1;

            foreach (var country in countries)
            {
                var countryDetails = allDetails
                    .Where(d => d.CountryId == country.Id
                             && d.Year >= startYear
                             && d.Year <= currentYear)
                    .ToList();

                var yearDetails = Enumerable.Range(startYear, years)
                    .Select(year =>
                    {
                        var details = countryDetails
                            .Where(d => d.Year == year)
                            .ToList();

                        int totalPoints = details.Sum(d => d.Points);

                        int participatingClubs = details
                            .Select(d => d.ClubId)
                            .Distinct()
                            .Count();

                        decimal points = participatingClubs > 0
                            ? (decimal)totalPoints / participatingClubs
                            : 0m;

                        return new CountryInternationalCompetitionRankingYearDetail
                        {
                            Year = year,
                            TotalPoints = totalPoints,
                            ParticipatingClubs = participatingClubs,
                            Points = points
                        };
                    })
                    .OrderByDescending(d => d.Year)
                    .ToList();

                ranking.Rows.Add(new CountryInternationalCompetitionRankingRow
                {
                    CountryId = country.Id,
                    CountryName = country.Name,
                    Points = yearDetails.Sum(d => d.Points),
                    YearDetails = yearDetails
                });
            }

            ranking.Rows = ranking.Rows
                .OrderByDescending(r => r.Points)
                .ThenBy(r => r.CountryName)
                .ToList();

            return ranking;
        }
    }
}
