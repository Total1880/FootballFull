using FootballFull.Models;
using FootballFull.Repositories.Interfaces;

namespace FootballFull.Repositories
{
    public class CompetitionRepository : IRepository<Competition>
    {
        public void Add(Competition item)
        {
            throw new NotImplementedException();
        }

        public IList<Competition> Create(IList<Competition> itemList, bool full)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(Competition updateItem)
        {
            throw new NotImplementedException();
        }

        public IList<Competition> Load()
        {
            var competitions = new List<Competition>
            {
                new Competition { Id = new Guid("fb60894f-07fd-4a59-aa0b-ecc16f193791"), Name = "Jupiler Pro League", CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Tier = 1 },
                new Competition { Id = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d"), Name = "Challenger Pro League", CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Tier = 2 },
                new Competition { Id = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d"), Name = "Eerste Nationale", CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Tier = 3},
            };

            return competitions;
        }
    }
}
