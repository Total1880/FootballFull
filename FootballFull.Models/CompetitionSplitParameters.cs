namespace FootballFull.Models
{
    public class CompetitionSplitParameters
    {
        public string Name { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
