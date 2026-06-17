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
        /// <summary>
        /// 1-10, 5 is neutraal, hoger is goed gevoel, lager is slecht
        /// </summary>
        [JsonIgnore]
        public int Morale { get; set; } = 5;
        [JsonIgnore]
        public DateTime HasTrainerSinceWeek { get; set; }
        [JsonIgnore]
        public DateTime HasFiredTrainerInWeek { get; set; }

    }
}
