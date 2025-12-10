using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public int TotalPoints(int year)
        {
            var totalPoints = 0;
            foreach (var point in PointsPerYear.Where(_ => _.Key >= year - 5)) {
                totalPoints += point.Value;
            }
            return totalPoints;
        }
        public void UpdatePoints(int year, int point) {
            if (PointsPerYear.ContainsKey(year))
                PointsPerYear[year] += point;
            else
                PointsPerYear.Add(year, point);
        }
    }
}
