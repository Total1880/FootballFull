using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        public List<Country> Load()
        {
            var countries = new List<Country>
            {
                new Country { Id = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Name = "Belgium" },
            };

            return countries;
        }
    }
}
