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
    public class CompetitionService : ICompetitionService
    {
        private readonly IRepository<Competition> _competitionRepository;

        public CompetitionService(IRepository<Competition> competitionRepository)
        {
            _competitionRepository = competitionRepository;
        }
        public IList<Competition> GetCompetitions()
        {
            return _competitionRepository.Load();
        }
    }
}
