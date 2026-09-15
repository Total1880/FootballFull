using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class FootballAssociation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public int Reputation { get; set; }
        public Guid CountryId { get; set; }
        [JsonIgnore]
        public Country Country { get; set; }
    }
}
