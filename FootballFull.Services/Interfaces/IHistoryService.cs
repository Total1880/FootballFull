using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IHistoryService
    {
        void SaveSeasonHistory();
        IList<CompetitionSeasonHistory> GetHistoryForCompetitionId(Guid competitionId);
        IList<ClubSeasonHistory> GetHistoryForClubId(Guid clubId);
    }
}
