using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IClubInternationalRankingService
    {
        IList<ClubInternationalRanking> GetAll();
        void SaveAll(IList<ClubInternationalRanking> clubs);
    }
}
