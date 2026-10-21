using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IInternationalRankingService
    {
        ClubInternationalRanking GetAll();
        void SaveAll();
        bool ProcessFixture(Fixture fixture, int year);
        decimal GetCountryPointsForXYears(Guid countryId, int years);
        CountryInternationalCompetitionRanking GetRanking(int years);
    }
}
