using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class Fixture
    {
        public Guid HomeTeamId { get; set; }
        public Club HomeTeam { get; set; }
        public Guid AwayTeamId { get; set; }
        public Club AwayTeam { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int MatchDay { get; set; }
        public Guid CompetitionId { get; set; }
    }
}
