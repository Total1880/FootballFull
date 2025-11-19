using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class SeasonService : ISeasonService
    {
        private ICompetitionRepository _competitionRepository;
        private IList<ClubLeagueCompetition> _clubLeagueCompetitions;
        private IList<ClubPerCompetition> _clubs;
        public IList<ClubLeagueCompetition> ClubLeagueCompetitions => _clubLeagueCompetitions;

        public SeasonService(ICompetitionRepository competitionRepository)
        {
            _competitionRepository = competitionRepository;
        }
        public void Initialize(IList<ClubPerCompetition> clubs)
        {
            _clubs = clubs;
            InitializeNewSeason();
        }
        public void InitializeNewSeason()
        {

            if (_clubLeagueCompetitions != null)
            {
                var promotionsAndRelegationPlaces = 2;
                var competitions = _competitionRepository.Load();
                // set promotions and relegations here

                foreach (var competition in competitions)
                {
                    var clubsInCompetition = _clubLeagueCompetitions
                        .Where(_ => _.CompetitionId == competition.Id)
                        .OrderByDescending(_ => _.Points)
                        .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
                        .ThenByDescending(_ => _.GoalsFor)
                        .ToList();

                    var relegatedClubs = clubsInCompetition.Skip(clubsInCompetition.Count - promotionsAndRelegationPlaces).ToList();
                    var promotedClubs = clubsInCompetition.Take(promotionsAndRelegationPlaces).ToList();

                    if (competition.Tier > 1)
                    {
                        foreach (var promotedClub in promotedClubs)
                        {
                            _clubs.FirstOrDefault(_ => _.ClubId == promotedClub.ClubId).CompetitionId = competitions.First(_ => _.Tier == competition.Tier - 1 && _.CountryId == competition.CountryId).Id;
                        }
                    }

                    if (competition.Tier < competitions.Max(_ => _.Tier))
                    {
                        foreach (var relegatedClub in relegatedClubs)
                        {
                            _clubs.FirstOrDefault(_ => _.ClubId == relegatedClub.ClubId).CompetitionId = competitions.First(_ => _.Tier == competition.Tier + 1 && _.CountryId == competition.CountryId).Id;
                        }
                    }
                }
            }
            _clubLeagueCompetitions = _clubs.Select(club => new ClubLeagueCompetition
            {
                ClubId = club.ClubId,
                Points = 0,
                GoalsFor = 0,
                GoalsAgainst = 0,
                CompetitionId = club.CompetitionId
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
