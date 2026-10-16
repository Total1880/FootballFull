using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class InternationalQualificationPlaces
    {
        public Guid CompetitionId { get; set; }
        public IDictionary<Guid, int> QualificationPlacesPerCountry { get; set; } = new Dictionary<Guid, int>();
    }
}
