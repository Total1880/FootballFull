using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
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
        private readonly IRepository<ClubLeagueCompetition> _clubLeagueCompetitionRepository;

        public ClubLeagueCompetitionService(IRepository<ClubLeagueCompetition> clubLeagueCompetitionRepository)
        {
            _clubLeagueCompetitionRepository = clubLeagueCompetitionRepository;
        }

        public IEnumerable<ClubLeagueCompetition> GetClubLeagueCompetitionsByCompetitionId(Guid competitionId)
        {
            return _clubLeagueCompetitionRepository.Load()
                .Where(c => c.CompetitionId == competitionId);
        }

        public IEnumerable<ClubLeagueCompetition> GetOrderedRanking(IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            return clubLeagueCompetitions
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => r.ClubId);
        }

        public bool SaveFullClubLeagueCompetitions(IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            _clubLeagueCompetitionRepository.Create(clubLeagueCompetitions, true);
            return true;
        }
    }
}
