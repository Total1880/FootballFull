using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ClubInternationalRankingRowYearDetail
    {
        public Guid ClubId { get; set; }
        public Guid CountryId { get; set; }
        public int Points { get; set; }
        public int Year { get; set; }
    }
}
