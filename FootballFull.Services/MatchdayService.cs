using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    /// <summary>
    /// All inputs for simulating one calendar day. Fixture collections are shared
    /// with the game so knockout progression is visible to subsequent matchdays.
    /// </summary>
    public sealed record MatchdayRequest(
        DateTime Date,
        Guid UserCountryId,
        IList<Competition> Competitions,
        IList<Fixture> LeagueFixtures,
        IList<Fixture> CupFixtures,
        IList<Fixture>? InternationalFixtures,
        WeekGamesToShow? GamesToShow);

    public sealed class MatchdayService : IMatchdayService
    {
        private readonly ISeasonService _seasonService;

        public MatchdayService(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        public void PlayDate(MatchdayRequest request)
        {
            // When finishing the season, all remaining national league games
            // must still be simulated, even if there is no player-facing result.
            var leaguePlayed = request.GamesToShow == null
                ? _seasonService.PlayMatchDay(request.LeagueFixtures, request.Date, false)
                : _seasonService.PlayMatchDay(request.LeagueFixtures, request.Date, false, request.UserCountryId);

            var cupPlayed = PlayCupGames(request);
            var internationalPlayed = PlayInternationalGames(request);

            if (request.GamesToShow != null)
            {
                request.GamesToShow.LeagueCompetition = leaguePlayed;
                request.GamesToShow.CupCompetition = cupPlayed;
                request.GamesToShow.InternationalCompetition = internationalPlayed;
            }

            _seasonService.UpdateWeekStats(request.UserCountryId, request.Date);
        }

        private bool PlayCupGames(MatchdayRequest request)
        {
            var playedForUser = false;
            foreach (var competition in request.Competitions.Where(c => c.Type == Competition.CompetitionType.Cup))
            {
                var roundFixtures = request.CupFixtures
                    .Where(f => f.CompetitionId == competition.Id && f.MatchDay == request.Date)
                    .ToList();

                if (roundFixtures.Count == 0)
                    continue;

                if (roundFixtures.Any(f => f.HomeTeam?.CountryId == request.UserCountryId ||
                                           f.AwayTeam?.CountryId == request.UserCountryId))
                    playedForUser = true;

                PlayKnockoutRound(roundFixtures, request.CupFixtures, request);
            }

            return playedForUser;
        }

        private bool PlayInternationalGames(MatchdayRequest request)
        {
            if (request.InternationalFixtures == null)
                return false;

            var roundFixtures = request.InternationalFixtures
                .Where(f => f.MatchDay == request.Date)
                .ToList();

            if (roundFixtures.Count == 0)
                return false;

            PlayKnockoutRound(roundFixtures, request.InternationalFixtures, request);
            return true;
        }

        private void PlayKnockoutRound(
            IList<Fixture> roundFixtures,
            IList<Fixture> competitionFixtures,
            MatchdayRequest request)
        {
            _seasonService.PlayMatchDay(roundFixtures, request.Date, true, request.UserCountryId, true);

            foreach (var fixture in roundFixtures)
                AdvanceKnockoutWinner(fixture, competitionFixtures);
        }

        private static void AdvanceKnockoutWinner(
            Fixture fixture,
            IList<Fixture> competitionFixtures)
        {
            var winner = fixture.HomeScore > fixture.AwayScore
                ? fixture.HomeTeam
                : fixture.AwayTeam;

            if (winner == null)
                return;

            var nextHomeFixture = competitionFixtures.FirstOrDefault(
                f => f.CupPreviousFixtureHomeTeam == fixture);

            if (nextHomeFixture != null)
            {
                if (nextHomeFixture.HomeTeamId == Guid.Empty)
                {
                    nextHomeFixture.HomeTeam = winner;
                    nextHomeFixture.HomeTeamId = winner.Id;
                }

                return;
            }

            var nextAwayFixture = competitionFixtures.FirstOrDefault(
                f => f.CupPreviousFixtureAwayTeam == fixture);

            if (nextAwayFixture != null && nextAwayFixture.AwayTeamId == Guid.Empty)
            {
                nextAwayFixture.AwayTeam = winner;
                nextAwayFixture.AwayTeamId = winner.Id;
            }
        }
    }
}
