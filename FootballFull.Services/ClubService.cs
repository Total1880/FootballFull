using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballFull.Services
{
    public class ClubService : IClubService
    {
        private readonly IRepository<Club> _clubRepository;

        public ClubService(IRepository<Club> clubRepository)
        {
            _clubRepository = clubRepository ?? throw new ArgumentNullException(nameof(clubRepository));
        }

        public void Add(Club club)
        {
            if (club == null)
                throw new ArgumentNullException(nameof(club));

            _clubRepository.Add(club);
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Club Id cannot be empty.", nameof(id));

            _clubRepository.Delete(id);
        }

        public Club? GetClubById(Guid clubId)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(clubId));

            return _clubRepository
                .Load()
                .FirstOrDefault(c => c.Id == clubId);
        }

        public IList<Club> GetClubs()
        {
            // Altijd opnieuw laden, zodat editor direct wijzigingen ziet
            return _clubRepository.Load();
        }

        public void Update(Club updatedClub)
        {
            if (updatedClub == null)
                throw new ArgumentNullException(nameof(updatedClub));
            if (updatedClub.Id == Guid.Empty)
                throw new ArgumentException("Club must have a valid Id to update.", nameof(updatedClub));

            _clubRepository.Update(updatedClub);
        }

        public void SaveAll(IList<Club> clubs)
        {
            _clubRepository.Create(clubs, true);
        }
    }
}
