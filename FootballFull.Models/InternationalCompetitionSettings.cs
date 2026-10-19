using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class InternationalCompetitionSettings
    {
        public Guid CompetitionId { get; set; }
        public int PointsForWin { get; set; }
        public int[] QualificationPlaces { get; set; }
    }
}
