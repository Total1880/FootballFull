using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonFinancialResultService
    {
        SeasonFinancialResult CalculateFinancialResults(int clubsCount, int competitionsCount, FootballAssociation association);
    }
}