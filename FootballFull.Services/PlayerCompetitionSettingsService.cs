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
        private ICompetitionService _competitionService;
        private ISaveDataService _saveDataService;

        public PlayerCompetitionSettingsService(IRepository<PlayerCompetitionSettings> repository, ICompetitionService competitionService, ISaveDataService saveDataService)
        {
            _repository = repository;
            _competitionService = competitionService;
            _saveDataService = saveDataService;
        }

        public bool AddSetting(PlayerCompetitionSettings playerCompetitionSettings)
        {
            _repository.Update(playerCompetitionSettings);

            return true;
        }

        public PlayerCompetitionSettings GetSettings()
        {
            var setting = _repository.Load().FirstOrDefault() ?? new PlayerCompetitionSettings();
            if (setting.PrizeMoneyPerCompetition == null || setting.PrizeMoneyPerCompetition.Count == 0)
            {
                setting.PrizeMoneyPerCompetition = new Dictionary<Guid, decimal>();
                foreach (var competition in _competitionService.GetCompetitionsForCountry(_saveDataService.SaveData.UserCountryId))
                    setting.PrizeMoneyPerCompetition.Add(competition.Id, 0);
            }

            if (setting.PrizeMoneyDistributionPerCompetition == null || setting.PrizeMoneyDistributionPerCompetition.Count == 0)
            {
                setting.PrizeMoneyDistributionPerCompetition = new Dictionary<Guid, PrizeMoneyDistribution>();
                foreach (var competition in _competitionService.GetCompetitionsForCountry(_saveDataService.SaveData.UserCountryId))
                    setting.PrizeMoneyDistributionPerCompetition.Add(competition.Id, 0);
            }
            return setting;
        }

        public bool UpdateSetting(PlayerCompetitionSettings playerCompetitionSettings)
        {
            _repository.Update(playerCompetitionSettings);

            return true;
        }
    }
}
