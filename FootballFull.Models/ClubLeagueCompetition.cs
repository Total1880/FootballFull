namespace FootballFull.Models
{
    public class ClubLeagueCompetition
    {
        public Club? Club { get; set; }
        public Guid ClubId { get; set; }
        public int MatchesPlayed { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public int Draw { get; set; }
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get => GoalsFor - GoalsAgainst; }
        public Competition Competition { get; set; }
        public Guid CompetitionId { get; set; }
    }
}
