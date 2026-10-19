namespace FootballFull.Models
{
    public class CountryInternationalCompetitionRanking
    {
        public IList<CountryInternationalCompetitionRankingRow> Rows { get; set; } = new List<CountryInternationalCompetitionRankingRow>();
    }
}