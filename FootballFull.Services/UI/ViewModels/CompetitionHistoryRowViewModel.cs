using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.UI.ViewModels
{
    public class CompetitionHistoryRowViewModel
    {
        public int Year { get; set; }
        public string? CompetitionName { get; set; }
        public string? ChampionClubName { get; set; }
        public string? RunnerUpClubName { get; set; }
    }
}
