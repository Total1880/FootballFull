using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public List<Competition> SubCompetitions { get; set; } = new List<Competition>();

        public enum CompetitionType
        {
            League,
            Cup,
            International
        }
        public IDictionary<int, DateTime> MatchDay { get; set; }
    }
}
