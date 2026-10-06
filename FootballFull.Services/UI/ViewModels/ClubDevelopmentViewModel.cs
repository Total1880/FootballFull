namespace FootballFull.Services.UI.ViewModels;

public class ClubDevelopmentViewModel
{
    public string ClubName { get; set; } = string.Empty;
    public decimal NetResult { get; set; }
    public decimal Investment { get; set; }
    public decimal Balance { get; set; }
    public decimal DevelopmentBudget { get; set; }
    public decimal SubsidyReceived { get; set; }
    public int StrengthBefore { get; set; }
    public int StrengthAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
}
