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
                new Country { Id = Guid.NewGuid(), Name = "Belgium" },
            };

            return countries;
        }
    }
}
