using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IFixtureService
    {
        IList<Fixture> Generate(IList<ClubPerCompetition> clubsPerCompetition);
        IList<Fixture> GenerateCupFixtures(IList<ClubPerCompetition> clubsPerCompetition, Competition competitionCup);
    }
}