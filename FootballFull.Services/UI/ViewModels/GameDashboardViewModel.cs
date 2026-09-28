namespace FootballFull.Services.UI.ViewModels;

public class GameDashboardViewModel
{
    public string AssociationName { get; init; } = string.Empty;
    public int SeasonStartYear { get; init; }
    public DateTime CurrentDate { get; init; }
    public decimal Balance { get; init; }
    public int Reputation { get; init; }
    public string ReputationDescription { get; init; } = string.Empty;
    public int ClubCount { get; init; }
    public int CompetitionCount { get; init; }
    public DateTime? NextMatchday { get; init; }
    public int NextMatchCount { get; init; }
}
