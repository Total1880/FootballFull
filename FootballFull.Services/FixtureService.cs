using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class FixtureService : IFixtureService
    {
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;

        public FixtureService(IClubService clubService, ICompetitionService competitionService)
        {
            _clubService = clubService;
            _competitionService = competitionService;
        }

        public IList<Fixture> Generate(IList<ClubPerCompetition> clubsPerCompetition, DateTime seasonStartDate)
        {
            var fixtures = new List<Fixture>();
            var firstMatchDay = FindFirstSaturday(seasonStartDate);

            foreach (var competitionId in clubsPerCompetition.Select(x => x.CompetitionId).Distinct())
            {
                var competition = _competitionService.GetCompetitionById(competitionId);
                if (competition == null || competition.Type != Competition.CompetitionType.League)
                    continue;

                var teams = clubsPerCompetition
                    .Where(x => x.CompetitionId == competitionId)
                    .Select(x => _clubService.GetClubById(x.ClubId))
                    .Where(x => x != null)
                    .DistinctBy(x => x.Id)
                    .ToList();

                if (teams.Count < 2)
                    continue;

                Shuffle(teams);
                fixtures.AddRange(GenerateLeagueFixtures(teams, competition, firstMatchDay));
            }

            return fixtures;
        }

        private IList<Fixture> GenerateLeagueFixtures(
            IList<Club> teams,
            Competition competition,
            DateTime firstMatchDay)
        {
            // Circle method needs an even participant count. Null is a bye.
            var rotation = teams.Cast<Club?>().ToList();
            if (rotation.Count % 2 != 0)
                rotation.Add(null);

            var roundsPerHalf = rotation.Count - 1;
            var matchDays = CreateAndSaveMatchDays(
                competition,
                firstMatchDay,
                roundsPerHalf * 2);

            var firstHalf = new List<Fixture>();

            for (var roundIndex = 0; roundIndex < roundsPerHalf; roundIndex++)
            {
                var roundNo = roundIndex + 1;

                for (var pairIndex = 0; pairIndex < rotation.Count / 2; pairIndex++)
                {
                    var first = rotation[pairIndex];
                    var second = rotation[rotation.Count - 1 - pairIndex];

                    if (first == null || second == null)
                        continue;

                    var switchHomeAndAway = (roundIndex + pairIndex) % 2 != 0;
                    var home = switchHomeAndAway ? second : first;
                    var away = switchHomeAndAway ? first : second;

                    firstHalf.Add(CreateFixture(
                        competition.Id,
                        roundNo,
                        matchDays[roundNo],
                        home,
                        away));
                }

                RotateTeams(rotation);
            }

            // Mirror the first half, guaranteeing one home and one away match
            // for every pair of clubs.
            var secondHalf = firstHalf.Select(firstLeg =>
            {
                var roundNo = firstLeg.RoundNo + roundsPerHalf;
                return CreateFixture(
                    competition.Id,
                    roundNo,
                    matchDays[roundNo],
                    firstLeg.AwayTeam,
                    firstLeg.HomeTeam);
            });

            return firstHalf.Concat(secondHalf).ToList();
        }

        private IDictionary<int, DateTime> CreateAndSaveMatchDays(
            Competition competition,
            DateTime firstMatchDay,
            int roundCount)
        {
            var matchDays = Enumerable.Range(1, roundCount).ToDictionary(
                roundNo => roundNo,
                roundNo => firstMatchDay.AddDays((roundNo - 1) * 7));

            // Always overwrite the dates; otherwise a new season reuses the
            // dates stored for the previous season.
            return _competitionService.UpdateMatchDays(competition.Id, matchDays);
        }

        private static Fixture CreateFixture(
            Guid competitionId,
            int roundNo,
            DateTime matchDay,
            Club home,
            Club away)
        {
            return new Fixture
            {
                CompetitionId = competitionId,
                RoundNo = roundNo,
                MatchDay = matchDay,
                HomeTeamId = home.Id,
                HomeTeam = home,
                AwayTeamId = away.Id,
                AwayTeam = away
            };
        }

        private static void RotateTeams(IList<Club?> teams)
        {
            var last = teams[^1];
            for (var index = teams.Count - 1; index > 1; index--)
                teams[index] = teams[index - 1];
            teams[1] = last;
        }

        private static DateTime FindFirstSaturday(DateTime seasonStartDate)
        {
            var daysUntilSaturday =
                ((int)DayOfWeek.Saturday - (int)seasonStartDate.DayOfWeek + 7) % 7;
            return seasonStartDate.AddDays(daysUntilSaturday);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (var index = list.Count - 1; index > 0; index--)
            {
                var randomIndex = Random.Shared.Next(index + 1);
                (list[index], list[randomIndex]) = (list[randomIndex], list[index]);
            }
        }

        public IList<Fixture> GenerateCupFixtures(
            IList<ClubPerCompetition> clubsPerCompetition,
            Competition competitionCup,
            DateTime seasonStartDate)
        {
            if (clubsPerCompetition == null || clubsPerCompetition.Count < 2)
                return new List<Fixture>();

            var teams = clubsPerCompetition
                .Where(x => x.CompetitionId == competitionCup.Id)
                .Select(x => _clubService.GetClubById(x.ClubId))
                .Where(x => x != null)
                .DistinctBy(x => x.Id)
                .ToList();

            return GenerateCup(teams, competitionCup, seasonStartDate);
        }

        private IList<Fixture> GenerateCup(
            IList<Club> teams,
            Competition competitionCup,
            DateTime seasonStartDate)
        {
            if (teams == null || teams.Count < 2)
                return new List<Fixture>();

            var fixtures = new List<Fixture>();
            var realTeams = teams.ToList();
            Shuffle(realTeams);

            var bracketSize = 1;
            while (bracketSize < realTeams.Count)
                bracketSize *= 2;

            var byeCount = bracketSize - realTeams.Count;
            var roundCount = (int)Math.Log2(bracketSize);
            var firstMatchDay = FindFirstSaturday(seasonStartDate);
            var matchDays = CreateAndSaveMatchDays(
                competitionCup,
                firstMatchDay,
                roundCount);

            var bye = new Club { Id = Guid.Empty, Name = "Bye" };
            var currentRound = new List<Fixture>();
            var teamIndex = 0;

            for (var index = 0; index < byeCount; index++)
            {
                var fixture = CreateFixture(
                    competitionCup.Id,
                    1,
                    matchDays[1],
                    realTeams[teamIndex++],
                    bye);
                fixtures.Add(fixture);
                currentRound.Add(fixture);
            }

            while (teamIndex < realTeams.Count)
            {
                var fixture = CreateFixture(
                    competitionCup.Id,
                    1,
                    matchDays[1],
                    realTeams[teamIndex++],
                    realTeams[teamIndex++]);
                fixtures.Add(fixture);
                currentRound.Add(fixture);
            }

            for (var roundNo = 2; currentRound.Count > 1; roundNo++)
            {
                var nextRound = new List<Fixture>();

                for (var index = 0; index < currentRound.Count; index += 2)
                {
                    var fixture = new Fixture
                    {
                        CompetitionId = competitionCup.Id,
                        RoundNo = roundNo,
                        MatchDay = matchDays[roundNo],
                        CupPreviousFixtureHomeTeam = currentRound[index],
                        CupPreviousFixtureAwayTeam = currentRound[index + 1]
                    };
                    fixtures.Add(fixture);
                    nextRound.Add(fixture);
                }

                currentRound = nextRound;
            }

            return fixtures;
        }
    }
}
