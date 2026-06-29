using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class CompetitionRules
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Competition Competition { get; set; }
        public int PromotionPlaces { get; set; }
        public int RelegationPlaces { get; set; }
        public Competition? PromotionTo { get; set; }
        public Competition? RelegationTo { get; set; }
    }
}
