namespace FootballFull.Models
{
    public class CompetitionSeasonHistory
    {
        public Guid Id { get; set; }
        public Guid CompetitionId { get; set; }
        public string CompetitionName { get; set; }
        public int Year { get; set; }

        public Guid ChampionClubId { get; set; }
        public string? ChampionClubName { get; set; }
        public Guid RunnerUpClubId { get; set; }
        public string? RunnerUpClubName { get; set; }
    }
}
