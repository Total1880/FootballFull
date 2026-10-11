namespace FootballFull.Models
{
    public class CompetitionPrizeMoneySeason
    {
        public int Year { get; set; }
        public CompetitionPrizeMoneySettings Settings { get; set; } = new();
        public bool IsPaid { get; set; }
        public List<ClubPrizeMoney> Payments { get; set; } = new();
    }
}
