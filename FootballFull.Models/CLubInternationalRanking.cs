using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class CLubInternationalRanking
    {
        public Guid ClubId { get; set; }
        public Club Club { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; }

        public IDictionary<int, int> PointsPerYear { get => _pointsPerYear; }
        private IDictionary<int, int> _pointsPerYear;
        public int TotalPoints(int year)
        {
            var totalPoints = 0;
            foreach (var point in PointsPerYear.Where(_ => _.Key >= year - 5)) {
                totalPoints += point.Value;
            }
            return totalPoints;
        }

        public void UpdatePoints(int year, int point) {
            if (_pointsPerYear.ContainsKey(year))
                _pointsPerYear[year] += point;
            else
                _pointsPerYear.Add(year, point);
        }
    }
}
