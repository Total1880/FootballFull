using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

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

        public Club FindParentClub(Guid feederClubId)
        {
            if (feederClubId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(feederClubId));

            return _clubRepository
                .Load()
                .FirstOrDefault(c => c.FeederClubId == feederClubId);
        }

        public bool DeleteAllSplitParametersForThisCountry(Guid countryId)
        {
            var clubs = _clubRepository.Load().Where(c => c.CountryId == countryId).ToList();
            foreach (var club in clubs)
            {
                club.CompetitionSplitParameters = null;
                _clubRepository.Update(club);
            }
            return true;
        }

        public bool DeleteAllFeederClubsForThisCountry(Guid countryId)
        {
            var clubs = _clubRepository.Load().Where(c => c.CountryId == countryId && (c.FeederClubId != null || c.FeederClubId != Guid.Empty)).ToList();
            foreach(var club in clubs)
            {
                _clubRepository.Delete((Guid)club.FeederClubId);
                club.FeederClubId = null;
                _clubRepository.Update(club);
            }
            return true;
        }

        public IList<Club> GetEndOfSeasonRequestClubs(Guid countryId, int numberOfClubs, List<Guid> existingClubIds)
        {
            var list = new List<Club>();
            var clubs = _clubRepository.Load().Where(c => c.CountryId == countryId && !existingClubIds.Contains(c.Id)).ToList();

            for (int i = 0; i < numberOfClubs && i < clubs.Count; i++)
            {
                var randomClub = clubs[new Random().Next(clubs.Count)];
                randomClub.Strength = OlavFramework.Configuration.MinStrength;
                _clubRepository.Update(randomClub);
                list.Add(randomClub);
                clubs.Remove(randomClub); // Remove the selected club to avoid duplicates
            }
            return list;
        }
    }
}
