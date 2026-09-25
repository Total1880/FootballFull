using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class FootballAssociationService : IFootballAssociationsService
    {
        private readonly IRepository<FootballAssociation> _repository;
        public FootballAssociationService(IRepository<FootballAssociation> repository)
        {
            _repository = repository;
        }
        public void Add(FootballAssociation association)
        {
            if (association.Id == null || association.Id == Guid.Empty)
                association.Id = Guid.NewGuid();

            _repository.Add(association);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }

        public IList<FootballAssociation> GetAll()
        {
            return _repository.Load();
        }

        public FootballAssociation GetByCountryId(Guid countryId)
        {
            return _repository.Load().FirstOrDefault(_ => _.CountryId == countryId);
        }

        public FootballAssociation GetById(Guid id)
        {
            return _repository.Load().FirstOrDefault(_ => _.Id == id);
        }

        public void Update(FootballAssociation association)
        {
            _repository.Update(association);
        }
    }
}
