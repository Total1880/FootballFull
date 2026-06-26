using FootballFull.Models;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{


    public class ClubLeagueCompetitionService : IClubLeagueCompetitionService
    {
        public IEnumerable<ClubLeagueCompetition> GetOrderedRanking(IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            return clubLeagueCompetitions
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => r.ClubId);
        }
    }
}
