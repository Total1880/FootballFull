using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.UI.ViewModels
{
    public class PlayerCompetitionSettingsViewModel
    {
        public Dictionary<string, decimal> PrizeMoneyPerCompetition = new Dictionary<string, decimal>();
        public bool AutoAddClub;
        public bool AutoAddCompetition;
        public int AutoAddCompetitionThreshold;
        public bool AutoAddFeederClub ;
    }
}
