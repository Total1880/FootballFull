using FootballFull.Services.UI.ViewModels;

namespace FootballFull.Services.UI;

public interface IGameUI
{
    MainMenuChoice ShowMainMenu(GameDashboardViewModel dashboard);
    void ShowTable(CompetitionTableViewModel competitionTable);

    void ShowMessage(string title, string message);
}
