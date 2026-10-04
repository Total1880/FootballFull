namespace FootballFull.Services.UI.ViewModels;

public class SeasonFinancialResultViewModel
{
    public string AssociationName { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public int Reputation { get; init; }
    public string ReputationDescription { get; init; } = string.Empty;
    public decimal ClubIncome { get; init; }
    public decimal ReputationIncome { get; init; }
    public decimal BonusIncome { get; init; }
    public decimal ClubCosts { get; init; }
    public decimal CompetitionCosts { get; init; }
    public decimal OrganisationCosts { get; init; }
    public int ReputationChange { get; init; }
    public decimal NetResult { get; init; }
}
