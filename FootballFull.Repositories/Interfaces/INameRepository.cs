using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories.Interfaces
{
    public interface INameRepository
    {
        string GetRandomFirstName(Guid CountryId);
        string GetRandomLastName(Guid CountryId);
    }
}
