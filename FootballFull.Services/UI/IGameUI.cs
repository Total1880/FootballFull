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
    Competition ChooseCompetitions(List<Competition> competitions);
    void ShowInternationRankings(List<CountryCoefficientRanking> rankings, int currentYear, bool waitForUser = false);
    void ShowSeasonEvent(SeasonEvent seasonEvent, bool waitForUser = false);
    void ShowSeasonFinancialResult(SeasonFinancialResult seasonFinancialResult, bool waitForUser = false);

    Club AskPlayerToSelectClub(IList<Club> applicants);
    bool AskYesNoQuestion(string question, bool defaultAnswer = false);
    string AskForInput(string question, string defaultAnswer = "");
    int AskForClubsToMove(int maximumClubsToMove, int currentClubCount, int minimumClubsToMove = 0);

    bool AskToCreateLowerDivision();
    IList<string> AskStarterClubNames(int numberOfClubs);
    int AskPlayerToSelectCountry(IList<Country> countries);
    string AskNewCountryName();
    void ShowNews(IList<NewsMessage> news);
}
