namespace FootballFull.Models
{
    public class Fixture
    {
        public Guid HomeTeamId { get; set; }
        public Club HomeTeam { get; set; }
        public Guid AwayTeamId { get; set; }
        public Club AwayTeam { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public DateTime MatchDay { get; set; }
        public int RoundNo { get; set; }
        public Guid CompetitionId { get; set; }
        public Fixture? CupPreviousFixtureHomeTeam { get; set; }
        public Fixture? CupPreviousFixtureAwayTeam { get; set; }
    }
}
