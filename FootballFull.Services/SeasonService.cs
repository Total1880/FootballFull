using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class SeasonService : ISeasonService
    {
        private IList<ClubLeagueCompetition> _clubLeagueCompetitions;
        public IList<ClubLeagueCompetition> ClubLeagueCompetitions => _clubLeagueCompetitions;

        public void InitializeNewSeason(IList<Club> clubs)
        {
            _clubLeagueCompetitions = clubs.Select(club => new ClubLeagueCompetition
            {
                ClubId = club.Id,
                Points = 0,
                GoalsFor = 0,
                GoalsAgainst = 0
            }).ToList();
        }

        public void PlayMatchDay(IList<Fixture> fixtures, int matchDay)
        {
            if (_clubLeagueCompetitions == null) throw new InvalidOperationException("Club league competitions not initialized.");
            foreach (var fixture in fixtures.Where(_ => _.MatchDay == matchDay))
            {
                fixture.HomeScore = Random.Shared.Next(0, 5) - fixture.AwayTeam.Strength;
                fixture.AwayScore = Random.Shared.Next(0, 5) - fixture.HomeTeam.Strength;

                while (fixture.HomeScore < 0 || fixture.AwayScore < 0)
                {
                    fixture.HomeScore++;
                    fixture.AwayScore++;
                }

                if (fixture.HomeScore > fixture.AwayScore)
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 3);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 0);
                }
                else if (fixture.HomeScore < fixture.AwayScore)
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 0);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 3);
                }
                else
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 1);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 1);
                }
            }
        }

        private void UpdateClubStats(Guid id, int awayScore, int homeScore, int v)
        {
            var record = _clubLeagueCompetitions.First(_ => _.ClubId == id);
            record.GoalsFor += awayScore;
            record.GoalsAgainst += homeScore;
            record.Points += v;
        }
    }
}
