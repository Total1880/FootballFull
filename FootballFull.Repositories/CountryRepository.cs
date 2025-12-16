using FootballFull.Models;
using FootballFull.Repositories.Interfaces;

namespace FootballFull.Repositories
{
    public class CountryRepository : IRepository<Country>
    {
        public void Add(Country item)
        {
            throw new NotImplementedException();
        }

        public IList<Country> Create(IList<Country> itemList, bool full)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(Country updateItem)
        {
            throw new NotImplementedException();
        }

        public IList<Country> Load()
        {
            var countries = new List<Country>
            {
                new Country { Id = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Name = "Belgium" },
            };

            return countries;
        }
    }
}
