namespace FootballFull.Services.UI.ViewModels;

public class FixtureViewModel
{
    public string HomeTeamName { get; init; } = string.Empty;
    public string AwayTeamName { get; init; } = string.Empty;
    public int HomeScore { get; init; }
    public int AwayScore { get; init; }
    public DateTime MatchDay { get; init; }
}
