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
    public class CompetitionSplitParametersService : ICompetitionSplitParametersService
    {
        private readonly IRepository<CompetitionSplitParameters> _competitionSplitParametersRepository;

        public CompetitionSplitParametersService(IRepository<CompetitionSplitParameters> competitionSplitParametersRepository)
        {
            _competitionSplitParametersRepository = competitionSplitParametersRepository;
        }

        public void Add(CompetitionSplitParameters competitionSplitParameters)
        {
            _competitionSplitParametersRepository.Add(competitionSplitParameters);
        }

        public void Delete(Guid id)
        {
            _competitionSplitParametersRepository.Delete(id);
        }

        public IList<CompetitionSplitParameters> GetCompetitionSplitParameters()
        {
            return _competitionSplitParametersRepository.Load();
        }

        public CompetitionSplitParameters? GetCompetitionSplitParametersById(Guid id)
        {
            return _competitionSplitParametersRepository.Load().FirstOrDefault(c => c.Id == id);
        }

        public void SaveAll(IList<CompetitionSplitParameters> competitionSplitParameters)
        {
            _competitionSplitParametersRepository.Create(competitionSplitParameters, true);
        }

        public void Update(CompetitionSplitParameters competitionSplitParameters)
        {
            _competitionSplitParametersRepository.Update(competitionSplitParameters);
        }
    }
}
