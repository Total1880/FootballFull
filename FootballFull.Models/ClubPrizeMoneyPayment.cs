namespace FootballFull.Models
{
    public class ClubPrizeMoneyPayment
    {
        public Guid CompetitionId { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
