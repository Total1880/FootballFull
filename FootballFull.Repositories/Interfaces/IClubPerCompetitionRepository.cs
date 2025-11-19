using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories.Interfaces
{
    public interface IClubPerCompetitionRepository : IRepository<ClubPerCompetition>
    {
        public void Delete(Guid clubId, Guid competitionId);
    }
}
