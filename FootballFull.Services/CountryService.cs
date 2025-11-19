using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<Country> _countryRepository;

        public CountryService(IRepository<Country> countryRepository)
        {
            _countryRepository = countryRepository;
        }
        public void Add(Country country)
        {
            _countryRepository.Add(country);
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IList<Country> GetCountries()
        {
            throw new NotImplementedException();
        }

        public Country GetCountryById(Guid countryId)
        {
            throw new NotImplementedException();
        }

        public void Update(Country updatedCountry)
        {
            throw new NotImplementedException();
        }
    }
}
