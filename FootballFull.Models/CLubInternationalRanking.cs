using System.Text.Json.Serialization;

namespace FootballFull.Models
{
    public class ClubInternationalRanking
    {
        public Guid Id { get; set; }
        public IList<ClubInternationalRankingRow> Rows { get; set; } = new List<ClubInternationalRankingRow>();
    }
}
