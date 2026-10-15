using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ExtraPromotionPlace
    {
        public Guid CompetitionId { get; set; }
        public Guid RemovedClubId { get; set; }
    }
}
