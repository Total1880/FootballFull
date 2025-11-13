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
                new Competition { Id = new Guid("fb60894f-07fd-4a59-aa0b-ecc16f193791"), Name = "Belgian Pro League", CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Tier = 1 },
            };

            return competitions;
        }
    }
}
