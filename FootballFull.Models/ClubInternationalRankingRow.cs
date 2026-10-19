using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ClubInternationalRankingRow
    {
        public IList<ClubInternationalRankingRowYearDetail> YearDetails { get; set; } = new List<ClubInternationalRankingRowYearDetail>();
    }
}
