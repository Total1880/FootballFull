using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class CompetitionRules
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Competition Competition { get; set; }
        public Guid CompetitionId { get; set; }
        public Guid CompetitionPromotionToId { get; set; }
        public Guid CompetitionRelegationToId { get; set; }
        public int PromotionPlaces { get; set; }
        public int RelegationPlaces { get; set; }
        [JsonIgnore]
        public Competition? PromotionTo { get; set; }
        [JsonIgnore]
        public Competition? RelegationTo { get; set; }
    }
}
