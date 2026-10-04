namespace FootballFull.Services.UI.ViewModels;

public class CountryRankingViewModel
{
    public string CountryName { get; init; } = string.Empty;
    public double FiveYearCoefficient { get; init; }
    public IReadOnlyDictionary<int, double> CoefficientPerYear { get; init; } = new Dictionary<int, double>();
    public IReadOnlyDictionary<int, int> ClubsParticipatingPerYear { get; init; } = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> RawPointsPerYear { get; init; } = new Dictionary<int, int>();
}
