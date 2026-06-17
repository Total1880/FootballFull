using System.ComponentModel.DataAnnotations;

namespace FootballFull.Models
{
    public class NewsMessage
    {
        [Required]
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid? ClubId { get; set; }
        public Guid? CompetitionId { get; set; }
        public Guid? CountryId { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
    }
}
