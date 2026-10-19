using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IInternationalRankingService
    {
        ClubInternationalRanking GetAll();
        void SaveAll();
        bool ProcessFixture(Fixture fixture, int year);
        int GetCountryPointsForXYears(Guid countryId, int years);
    }
}
