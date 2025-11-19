using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface ICountryService
    {
        Country GetCountryById(Guid countryId);
        IList<Country> GetCountries();
        void Add(Country country);
        void Update(Country updatedCountry);
        void Delete(Guid id);
    }
}
