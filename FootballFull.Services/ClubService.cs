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
        private readonly IRepository<Club> _clubRepository;

        private IList<Club> _clubs;

        public ClubService(IRepository<Club> clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public void Add(Club club)
        {
            _clubRepository.Add(club);
        }

        public void Delete(Guid id)
        {
            _clubRepository.Delete(id);
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

        public void Update(Club updatedClub)
        {
            _clubRepository.Update(updatedClub);
        }
    }
}
