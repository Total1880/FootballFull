using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IClubService
    {
        Club GetClubById(Guid clubId);
        IList<Club> GetClubs();
        void Add(Club club);
        void Update(Club updatedClub);
        void Delete(Guid id);
    }
}