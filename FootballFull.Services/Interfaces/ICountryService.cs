using FootballFull.Models;

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
