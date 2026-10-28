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
            setting.PrizeMoneyPerCompetition ??= new Dictionary<Guid, decimal>();
            setting.PrizeMoneyDistributionPerCompetition ??= new Dictionary<Guid, PrizeMoneyDistribution>();

            foreach (var competition in _competitionService.GetCompetitionsForCountry(_saveDataService.SaveData.UserCountryId)
                .Where(c => c.Type == Competition.CompetitionType.League || c.Type == Competition.CompetitionType.Cup))
            {
                if (!setting.PrizeMoneyPerCompetition.ContainsKey(competition.Id))
                    setting.PrizeMoneyPerCompetition[competition.Id] = 0m;

                if (!setting.PrizeMoneyDistributionPerCompetition.ContainsKey(competition.Id))
                    setting.PrizeMoneyDistributionPerCompetition[competition.Id] =
                        competition.Type == Competition.CompetitionType.League
                            ? PrizeMoneyDistribution.Equal
                            : PrizeMoneyDistribution.WinnerOnly;
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
