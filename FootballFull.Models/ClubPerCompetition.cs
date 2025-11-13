using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ClubPerCompetition
    {
        public Guid ClubId { get; set; }
        public Guid CompetitionId { get; set; }
    }
}
