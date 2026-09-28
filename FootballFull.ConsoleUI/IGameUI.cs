using FootballFull.ConsoleUI.ViewModels;
using FootballFull.Models;

namespace FootballFull.ConsoleUI
{
    public interface IGameUI
    {
        MainMenuChoice ShowMainMenu(GameDashboardViewModel dashboard);

        void ShowLeagueTable(CompetitionTableViewModel table);

        void ShowFinancialReport(FinancialReportViewModel report);

        void ShowMessage(string title, string message);

        void ShowNews(IList<NewsMessage> messages);

        EndOfSeasonChoice ShowEndOfSeasonMenu(
            EndOfSeasonViewModel viewModel);

        Club SelectApplicantClub(IList<Club> applicants);
    }
}
