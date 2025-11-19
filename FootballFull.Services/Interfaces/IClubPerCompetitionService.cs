using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IClubCompetitionService
    {
        void AddClubToCompetition(Guid clubId, Guid competitionId);
        void RemoveClubFromCompetition(Guid clubId, Guid competitionId);

        IList<Club> GetClubsForCompetition(Guid competitionId);
        IList<Competition> GetCompetitionsForClub(Guid clubId);
    }
}
