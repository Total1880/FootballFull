namespace FootballFull.Models
{
    public class CountryInternationalCompetitionRankingRow
    {
        public Guid CountryId { get; set; }
        public string CountryName { get; set; }

        public decimal Points { get; set; }

        public IList<CountryInternationalCompetitionRankingYearDetail> YearDetails { get; set; }
            = new List<CountryInternationalCompetitionRankingYearDetail>();
    }
}
