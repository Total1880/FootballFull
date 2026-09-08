using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class Competition
    {
        private List<Competition> _subCompetitions = new();

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Country Country { get; set; }
        public Guid CountryId { get; set; }
        public int Tier { get; set; }
        public int Strength { get; set; }
        public CompetitionType Type { get; set; }
        [JsonIgnore]
        public List<Competition> SubCompetitions => _subCompetitions;
        public List<Guid> SubCompetitionIds { get; set; } = new();

        public enum CompetitionType
        {
            League,
            Cup,
            International,
            ParentCompetition
        }
        public IDictionary<int, DateTime> MatchDay { get; set; }
        public List<CompetitionSplitParameters> SplitParameters { get; set; } = new List<CompetitionSplitParameters>();
    }
}
