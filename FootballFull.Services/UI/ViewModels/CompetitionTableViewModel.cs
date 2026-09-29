using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.UI.ViewModels
{
    public class CompetitionTableViewModel
    {
        public string CompetitionName { get; set; }
        public IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; set; }
    }
}
