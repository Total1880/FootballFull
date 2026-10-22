using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IPlayerCompetitionSettingsService
    {
        public bool AddSetting(PlayerCompetitionSettings playerCompetitionSettings);
        public bool UpdateSetting(PlayerCompetitionSettings playerCompetitionSettings);
        public PlayerCompetitionSettings GetSettings();
    }
}
