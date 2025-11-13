using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ClubLeagueCompetition
    {
        public Club? Club { get; set; }
        public Guid ClubId { get; set; }
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public Competition Competition { get; set; }
        public int CompetitionId { get; set; }
    }
}
