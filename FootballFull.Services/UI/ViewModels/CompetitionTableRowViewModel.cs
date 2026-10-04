namespace FootballFull.Services.UI.ViewModels;

public class CompetitionTableRowViewModel
{
    public string ClubName { get; init; } = string.Empty;
    public int MatchesPlayed { get; init; }
    public int Won { get; init; }
    public int Draw { get; init; }
    public int Lost { get; init; }
    public int GoalsFor { get; init; }
    public int GoalsAgainst { get; init; }
    public int GoalDifference { get; init; }
    public int Points { get; init; }
}
