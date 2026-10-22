using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class PlayerCompetitionSettings
    {
        public Dictionary<Guid, decimal> PrizeMoneyPerCompetition { get; set; } = new Dictionary<Guid, decimal>();
        public bool AutoAddClub { get; set; } = false;
        public bool AutoAddCompetition { get; set; } = false;
        public int AutoAddCompetitionThreshold { get; set; } = 2;
        public bool AutoAddFeederClub { get; set; } = false;
    }
}
