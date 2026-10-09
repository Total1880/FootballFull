namespace FootballFull.Models
{
    public class ClubSeasonHistory
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public Guid CompetitionId { get; set; }
        public int Year { get; set; }

        public int Position { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
    }
}
