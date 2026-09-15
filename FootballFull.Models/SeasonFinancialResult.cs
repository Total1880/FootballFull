namespace FootballFull.Models
{
    public class SeasonFinancialResult
    {
        public decimal ClubIncome { get; set; }
        public decimal ReputationIncome { get; set; }
        public decimal BonusIncome { get; set; }

        public decimal ClubCosts { get; set; }
        public decimal CompetitionCosts { get; set; }
        public decimal OrganisationCosts { get; set; }

        public int ReputationChange { get; set; }

        public decimal NetResult =>
            ClubIncome +
            ReputationIncome +
            BonusIncome -
            ClubCosts -
            CompetitionCosts -
            OrganisationCosts;
    }
}
