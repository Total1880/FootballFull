using FootballFull.Models;

namespace FootballFull.Services
{
    public class SeasonService
    {

        public SeasonService()
        {

        }

        public void PlayMatchDay(IList<Fixture> fixtures, int matchDay)
        {
            foreach (var fixture in fixtures.Where(_ => _.MatchDay == matchDay))
            {
                fixture.HomeScore = Random.Shared.Next(0, 5)-fixture.AwayTeam.Strength;
                fixture.AwayScore = Random.Shared.Next(0, 5)-fixture.HomeTeam.Strength;

                while (fixture.HomeScore < 0 || fixture.AwayScore < 0)
                {
                    fixture.HomeScore++;
                    fixture.AwayScore++;
                }
            }
        }

        public void Run()
        {

        }
    }
}
