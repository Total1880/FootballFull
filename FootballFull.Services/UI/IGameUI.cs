using FootballFull.Services.UI.ViewModels;

namespace FootballFull.Services.UI;

public interface IGameUI
{
    MainMenuChoice ShowMainMenu(GameDashboardViewModel dashboard);
    void ShowTable(CompetitionTableViewModel competitionTable, bool waitForUser = false);

    void ShowMessage(string title, string message, bool waitForUser = false);
    void ShowResults(IReadOnlyList<FixtureViewModel> fixtures, bool waitForUser = false);
    void ShowFixtures(IReadOnlyList<FixtureViewModel> fixtures, bool waitForUser = false);
    Guid? ChooseCompetitions(IReadOnlyList<SelectionOptionViewModel> competitions);
    void ShowInternationRankings(IReadOnlyList<CountryRankingViewModel> rankings, int currentYear, bool waitForUser = false);
    void ShowSeasonEvent(SeasonEventViewModel seasonEvent, bool waitForUser = false);
    void ShowSeasonFinancialResult(SeasonFinancialResultViewModel seasonFinancialResult, bool waitForUser = false);

    Guid AskPlayerToSelectClub(IReadOnlyList<SelectionOptionViewModel> applicants);
    bool AskYesNoQuestion(string question, bool defaultAnswer = false);
    string AskForInput(string question, string defaultAnswer = "");
    int AskForClubsToMove(int maximumClubsToMove, int currentClubCount, int minimumClubsToMove = 0);

    bool AskToCreateLowerDivision();
    IList<string> AskStarterClubNames(int numberOfClubs);
    int AskPlayerToSelectCountry(IReadOnlyList<SelectionOptionViewModel> countries);
    string AskNewCountryName();
    void ShowClubDevelopment(IReadOnlyList<ClubDevelopmentViewModel> clubs, bool waitForUser = false);
    void ShowNews(IReadOnlyList<NewsMessageViewModel> news);
    decimal AskForSubsidyAmount(SelectSubsidyClubViewModel viewModel);
    void ShowCompetitionHistory(CompetitionHistoryViewModel competitionHistoryViewModel);
}
