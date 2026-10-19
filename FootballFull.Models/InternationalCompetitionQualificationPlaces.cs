using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class InternationalCompetitionQualificationPlaces
    {
        public Guid CompetitionId { get; set; }
        public IList<InternationalCompetitionQualificationPlacesRow> Rows { get; set; } = new List<InternationalCompetitionQualificationPlacesRow>();
    }
}
