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
    public class InternationalCompetitionSettingsService : IInternationalCompetitionSettingsService
    {
        private readonly IRepository<InternationalCompetitionSettings> _repository;
        private IList<InternationalCompetitionSettings> _settings;

        public InternationalCompetitionSettingsService(IRepository<InternationalCompetitionSettings> repository)
        {
            _repository = repository;
            _settings = _repository.Load();
        }
        public InternationalCompetitionSettings GetSettings(Guid competitionId)
        {
            return _settings.FirstOrDefault(s => s.CompetitionId == competitionId);
        }

        public void SaveSettings(InternationalCompetitionSettings settings)
        {
            _repository.Add(settings);
            _settings.Add(settings);
        }
    }
}
