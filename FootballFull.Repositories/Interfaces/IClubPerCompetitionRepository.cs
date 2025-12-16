using FootballFull.Models;

namespace FootballFull.Repositories.Interfaces
{
    public interface IClubPerCompetitionRepository : IRepository<ClubPerCompetition>
    {
        public void Delete(Guid clubId, Guid competitionId);
    }
}
