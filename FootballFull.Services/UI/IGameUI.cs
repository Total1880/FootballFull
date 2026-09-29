using FootballFull.Models;
using FootballFull.Services.UI.ViewModels;

namespace FootballFull.Services.UI;

public interface IGameUI
{
    MainMenuChoice ShowMainMenu(GameDashboardViewModel dashboard);
    void ShowTable(CompetitionTableViewModel competitionTable, bool waitForUser = false);

    void ShowMessage(string title, string message, bool waitForUser = false);
    void ShowResults(List<Fixture> fixtures, bool waitForUser = false);
    void ShowFixtures(List<Fixture> fixtures, bool waitForUser = false);
}
