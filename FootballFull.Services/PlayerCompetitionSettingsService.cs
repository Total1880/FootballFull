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
    public class PlayerCompetitionSettingsService : IPlayerCompetitionSettingsService
    {
        private IRepository<PlayerCompetitionSettings> _repository;

        public PlayerCompetitionSettingsService(IRepository<PlayerCompetitionSettings> repository)
        {
            _repository = repository;
        }

        public bool AddSetting(PlayerCompetitionSettings playerCompetitionSettings)
        {
            _repository.Update(playerCompetitionSettings);

            return true;
        }

        public PlayerCompetitionSettings GetSettings()
        {
            var setting = _repository.Load().FirstOrDefault() ?? new PlayerCompetitionSettings();
            return setting;
        }

        public bool UpdateSetting(PlayerCompetitionSettings playerCompetitionSettings)
        {
            _repository.Update(playerCompetitionSettings);

            return true;
        }
    }
}
