using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class InternationalRankingService : IInternationalRankingService
    {
        private ClubInternationalRanking _ranking;
        private readonly IRepository<ClubInternationalRanking> _repository;
        private readonly IInternationalCompetitionSettingsService _internationalCompetitionSettingsService;
        private readonly IClubService _clubService;

        public InternationalRankingService(
            IRepository<ClubInternationalRanking> repository,
            IInternationalCompetitionSettingsService internationalCompetitionSettingsService,
            IClubService clubService)
        {
            _repository = repository;
            _internationalCompetitionSettingsService = internationalCompetitionSettingsService;
            _clubService = clubService;

            _ranking = _repository.Load().FirstOrDefault();
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
            if(_ranking.Rows.Any(_ => _.YearDetails.Any(_ => _.ClubId == winner && _.Year == year)))
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

            return true;
        }

        public int GetCountryPointsForXYears(Guid countryId, int years)
        {
            return _ranking.Rows
                .SelectMany(row => row.YearDetails)
                .Where(detail => detail.CountryId == countryId)
                .OrderByDescending(detail => detail.Year)
                .Take(years)
                .Sum(detail => detail.Points);
        }
    }
}
