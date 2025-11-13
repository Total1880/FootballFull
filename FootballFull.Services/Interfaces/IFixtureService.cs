using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IFixtureService
    {
        IList<Fixture> Generate(IList<Club> teams);
    }
}