using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class Club
    {
        public required string Name { get; set; }
        public int Strength { get; set; }
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        [JsonIgnore]
        public string Last5Games { get; set; }
        [JsonIgnore]
        public int Momentum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Last5Games))
                    return 0;

                return Last5Games.Sum(c => c switch
                {
                    'W' => 3,
                    'D' => 1,
                    'L' => 0,
                    _ => 0
                });
            }
        }
    }
}
