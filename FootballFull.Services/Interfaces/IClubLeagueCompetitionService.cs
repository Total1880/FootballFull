using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IClubLeagueCompetitionService
    {
        public IEnumerable<ClubLeagueCompetition> GetOrderedRanking(IList<ClubLeagueCompetition> clubLeagueCompetitions);
    }
}
