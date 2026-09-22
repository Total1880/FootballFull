using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IClubFinancialService
    {
        decimal CalculateClubRevenue(
    Club club,
    Competition competition);

        decimal CalculateClubExpenses(
            Club club);

        ClubFinancialResult CalculateSeasonResult(
            Club club,
            Competition competition);
    }
}
