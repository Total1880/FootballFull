using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IInternationalQualificationPlacesService
    {
        void CalculateQualificationPlaces(int year);
        public int GetQualificationPlacesForCountryAndCompetition(Guid countryId, Guid competitionId);
    }
}
