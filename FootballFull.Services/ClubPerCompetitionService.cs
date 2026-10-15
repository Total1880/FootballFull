using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class ClubPerCompetitionService : IClubPerCompetitionService
    {
        private readonly IClubPerCompetitionRepository _linkRepository;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private IList<ClubPerCompetition> _clubPerCompetitions;

        public ClubPerCompetitionService(
            IClubPerCompetitionRepository linkRepository,
            IClubService clubService,
            ICompetitionService competitionService)
        {
            _linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
            _clubService = clubService ?? throw new ArgumentNullException(nameof(clubService));
            _competitionService = competitionService ?? throw new ArgumentNullException(nameof(competitionService));
            _clubPerCompetitions = _linkRepository.Load();
        }

        public void AddClubToCompetition(Guid clubId, Guid competitionId)
        {
            if (clubId == Guid.Empty) throw new ArgumentException("ClubId cannot be empty.", nameof(clubId));
            if (competitionId == Guid.Empty) throw new ArgumentException("CompetitionId cannot be empty.", nameof(competitionId));

            // optioneel: check of club/competition echt bestaan
            if (_clubService.GetClubById(clubId) == null)
                throw new InvalidOperationException($"Club {clubId} not found.");
            if (_competitionService.GetCompetitionById(competitionId) == null)
                throw new InvalidOperationException($"Competition {competitionId} not found.");

            var newLink = new ClubPerCompetition
            {
                ClubId = clubId,
                CompetitionId = competitionId
            };

            _linkRepository.Add(newLink);
            _clubPerCompetitions.Add(newLink);
        }

        public void RemoveClubFromCompetition(Guid clubId, Guid competitionId)
        {
            _linkRepository.Delete(clubId, competitionId);
            foreach(var link in _clubPerCompetitions.Where(l => l.ClubId == clubId && l.CompetitionId == competitionId).ToList() ) {
                _clubPerCompetitions.Remove(link);
            }
        }

        public IList<Club> GetClubsForCompetition(Guid competitionId)
        {
            var links = _clubPerCompetitions
                .Where(x => x.CompetitionId == competitionId)
                .ToList();

            var clubs = _clubService.GetClubs();

            return clubs
                .Where(c => links.Any(l => l.ClubId == c.Id))
                .ToList();
        }

        public IList<Competition> GetCompetitionsForClub(Guid clubId)
        {
            var links = _clubPerCompetitions
                .Where(x => x.ClubId == clubId)
                .ToList();

            var competitions = _competitionService.GetCompetitions();

            return competitions
                .Where(c => links.Any(l => l.CompetitionId == c.Id))
                .ToList();
        }

        public IList<ClubPerCompetition> GetAllClubPerCompetitions()
        {
            return _clubPerCompetitions;
        }

        public void SaveAll(IList<ClubPerCompetition> clubPerCompetition)
        {
            _linkRepository.Create(clubPerCompetition, true);
            _clubPerCompetitions = clubPerCompetition;
        }

        public IList<ClubPerCompetition> GetAllClubPerSpecificCompetitions(Guid competitionId)
        {
            return GetAllClubPerCompetitions().Where(_ => _.CompetitionId == competitionId).ToList();
        }

        public bool DeleteEverythingForCompetition(Guid competitionId)
        {
            var list = GetAllClubPerSpecificCompetitions(competitionId);
            foreach (var item in list)
            {
                RemoveClubFromCompetition(item.ClubId, item.CompetitionId);
            }
            return true;
        }

        public IList<ClubPerCompetition> GetAllClubPerCompetitionForCountry(Guid countryId)
        {
            var competitions = _competitionService.GetCompetitions().Where(c => c.CountryId == countryId).Select(c => c.Id).ToList();
            return GetAllClubPerCompetitions().Where(cpc => competitions.Contains(cpc.CompetitionId)).ToList();
        }

        public bool DeleteEverythingForClub(Guid clubId)
        {
            foreach(var item in GetAllClubPerCompetitions().Where(cpc => cpc.ClubId == clubId).ToList())
            {
                RemoveClubFromCompetition(item.ClubId, item.CompetitionId);
            } return true;  
        }
    }
}
