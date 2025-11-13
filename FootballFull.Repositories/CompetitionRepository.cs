using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class CompetitionRepository : ICompetitionRepository
    {
        public List<Competition> Load()
        {
            var competitions = new List<Competition>
            {
                new Competition { Id = Guid.NewGuid(), Name = "Belgian Pro League" },
            };

            return competitions;
        }
    }
}
