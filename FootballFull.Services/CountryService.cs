using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<Country> _countryRepository;

        public CountryService(IRepository<Country> countryRepository)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
        }

        public void Add(Country country)
        {
            if (country == null)
                throw new ArgumentNullException(nameof(country));

            _countryRepository.Add(country);
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            _countryRepository.Delete(id);
        }

        public IList<Country> GetCountries()
        {
            return _countryRepository.Load();
        }

        public Country? GetCountryById(Guid countryId)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(countryId));

            return _countryRepository
                .Load()
                .FirstOrDefault(c => c.Id == countryId);
        }

        public void Update(Country updatedCountry)
        {
            if (updatedCountry == null)
                throw new ArgumentNullException(nameof(updatedCountry));

            if (updatedCountry.Id == Guid.Empty)
                throw new ArgumentException("Country must have a valid Id to update.", nameof(updatedCountry));

            _countryRepository.Update(updatedCountry);
        }
    }
}
