namespace FootballFull.Models
{
    public class Competition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Country Country { get; set; }
        public Guid CountryId { get; set; }
        public int Tier { get; set; }
        public int Strength { get; set; }
        public CompetitionType Type { get; set; }

        public enum CompetitionType
        {
            League,
            Cup,
            International
        }
        public IDictionary<int, int> MatchDayPerWeek { get; set; }
    }
}
