using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class FootballAssociation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public int Reputation { get; set; }
        public string ReputationDescription => Reputation switch
        {
            < 10 => "Amateur",
            < 20 => "Regional",
            < 30 => "National",
            < 50 => "Professional",
            < 75 => "Prestigious",
            _ => "Elite"
        };
        public Guid CountryId { get; set; }
        [JsonIgnore]
        public Country Country { get; set; }
    }
}
