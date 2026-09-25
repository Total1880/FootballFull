using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IFootballAssociationsService
    {
        IList<FootballAssociation> GetAll();
        void Add(FootballAssociation association);
        void Delete(Guid id);
        FootballAssociation GetById(Guid id);
        FootballAssociation GetByCountryId(Guid countryId);
        void Update(FootballAssociation association);
    }
}
