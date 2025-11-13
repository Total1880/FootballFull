using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IClubService
    {
        Club GetClubById(Guid clubId);
        IList<Club> GetClubs();
    }
}