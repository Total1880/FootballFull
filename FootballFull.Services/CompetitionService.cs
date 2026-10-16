using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IRepository<Competition> _competitionRepository;

        private IList<Competition> _competitions;

        public CompetitionService(IRepository<Competition> competitionRepository, IClubPerCompetitionRepository clubPerCompetitionRepository, IRepository<CompetitionRules> competitionRulesRepository, IRepository<Club> clubRepository)
        {
            _competitionRepository = competitionRepository
                ?? throw new ArgumentNullException(nameof(competitionRepository));

            _competitions = _competitionRepository.Load();
        }

        public void Add(Competition competition)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));
            if (competition.MatchDay == null)
                competition.MatchDay = new Dictionary<int, DateTime>();

            _competitionRepository.Add(competition);
            _competitions.Add(competition);
        }

        public void Update(Competition competition)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));
            if (competition.Id == Guid.Empty)
                throw new ArgumentException("Competition must have a valid Id.");
            if (competition.MatchDay == null)
                competition.MatchDay = new Dictionary<int, DateTime>();

            _competitionRepository.Update(competition);
            var index = _competitions.ToList().FindIndex(c => c.Id == competition.Id);
            if (index >= 0)
                _competitions[index] = competition;
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.");

            _competitionRepository.Delete(id);
            _competitions.Remove(_competitions.First(_ => _.Id == id));
        }

        public IList<Competition> GetCompetitions(bool onlyActive = true)
        {
            if (onlyActive)
                return _competitions.Where(c => c.IsActive).ToList();
            return _competitions;
        }

        public Competition? GetCompetitionById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _competitions.First(_ => _.Id == id);
        }

        public void SaveAll()
        {
            _competitionRepository.Create(_competitions, true);
        }

        public IDictionary<int, DateTime> UpdateMatchDays(Guid competitionId, IDictionary<int, DateTime> matchDaysPerWeek)
        {
            var competition = GetCompetitionById(competitionId);
            if (competition == null)
                throw new ArgumentException("Competition not found.");
            if (competition.MatchDay == null)
                competition.MatchDay = new Dictionary<int, DateTime>();
            foreach (var kvp in matchDaysPerWeek)
            {
                if (competition.MatchDay.ContainsKey(kvp.Key))
                    competition.MatchDay[kvp.Key] = kvp.Value;
                else
                    competition.MatchDay.Add(kvp.Key, kvp.Value);
            }
            Update(competition);

            return competition.MatchDay;
        }

        public List<Competition> GetSubCompetitions(
    Competition competition)
        {
            var requestedIds = competition.SubCompetitionIds.ToHashSet();

            // Verwijder eerder geladen competities die niet meer gevraagd zijn.
            competition.SubCompetitions.RemoveAll(
                subCompetition => !requestedIds.Contains(subCompetition.Id));

            var loadedIds = competition.SubCompetitions
                .Select(subCompetition => subCompetition.Id)
                .ToHashSet();

            var missingIds = requestedIds
                .Where(id => !loadedIds.Contains(id))
                .ToList();

            if (missingIds.Count > 0)
            {
                var missingCompetitions =
                    _competitionRepository
                .Load().Where(c => missingIds.Contains(c.Id));

                competition.SubCompetitions.AddRange(missingCompetitions);
            }

            return competition.SubCompetitions;
        }

        public IList<Competition> GetCompetitionsForCountry(Guid userCountryId, bool onlyActive = true)
        {
            return GetCompetitions(onlyActive).Where(c => c.CountryId == userCountryId).ToList();
        }

        public IList<Competition> GetInternationalCompetitions(bool onlyActive = true)
        {
            return GetCompetitions(onlyActive).Where(c => c.Type == CompetitionType.International).ToList();
        }
    }
}
