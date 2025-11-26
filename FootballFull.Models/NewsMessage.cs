using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class NewsMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid? ClubId { get; set; }
        public Guid? CompetitionId { get; set; }
        public Guid? CountryId { get; set; }
        public int MatchDay { get; set; }
        public int Year { get; set; }
    }
}
