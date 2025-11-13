using FootballFull.Models;

namespace FootballFull.Repositories.Interfaces
{
    public interface IClubRepository
    {
        List<Club> Load(string path = "data/Clubs.json");
    }
}