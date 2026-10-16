using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class ClubInternationalRanking
    {
        public Guid ClubId { get; set; }
        public Club Club { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; }

        [JsonInclude]
        public IDictionary<int, int> PointsPerYear { get; private set; }
        = new Dictionary<int, int>();
    }
}
