using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface ICompetitionService
    {
        void Add(Competition competition);
        void Update(Competition competition);
        void Delete(Guid id);
        IList<Competition> GetCompetitions();
        Competition? GetCompetitionById(Guid id);
    }
}
