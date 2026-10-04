namespace FootballFull.Services.UI.ViewModels;

public class CompetitionTableViewModel
{
    public string CompetitionName { get; init; } = string.Empty;
    public IReadOnlyList<CompetitionTableRowViewModel> Rows { get; init; } = Array.Empty<CompetitionTableRowViewModel>();
}
