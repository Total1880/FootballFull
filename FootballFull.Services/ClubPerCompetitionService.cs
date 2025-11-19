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
    public class ClubPerCompetitionService : IClubPerCompetitionService
    {
        private readonly IRepository<ClubPerCompetition> _clubPerCompetitionRepository;

        public ClubPerCompetitionService(IRepository<ClubPerCompetition> clubPerCompetitionRepository)
        {
            _clubPerCompetitionRepository = clubPerCompetitionRepository;
        }
        public IList<ClubPerCompetition> GetClubsPerCompetition()
        {
            return _clubPerCompetitionRepository.Load();
        }
    }
}
