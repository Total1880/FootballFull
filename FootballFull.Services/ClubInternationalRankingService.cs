using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class ClubInternationalRankingService : IClubInternationalRankingService
    {
        private readonly IRepository<ClubInternationalRanking> _repository;
        public ClubInternationalRankingService(IRepository<ClubInternationalRanking> repository)
        {
                _repository = repository;
        }
        public IList<ClubInternationalRanking> GetAll()
        {
            return _repository.Load();
        }

        public void SaveAll(IList<ClubInternationalRanking> clubs)
        {
            _repository.Create(clubs, true);
        }
    }
}
