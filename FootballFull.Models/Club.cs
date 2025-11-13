namespace FootballFull.Models
{
    public class Club
    {
        public required string Name { get; set; }
        public int Strength { get; set; }
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
    }
}
