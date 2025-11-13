using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _clubRepository;

        private IList<Club> _clubs;

        public ClubService(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public Club GetClubById(Guid clubId)
        {
            return GetClubs().First(c => c.Id == clubId);
        }

        public IList<Club> GetClubs()
        {
            _clubs = _clubs ?? _clubRepository.Load();
            return _clubs;
        }
    }
}
