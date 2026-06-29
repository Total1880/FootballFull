using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IRepository<Competition> _competitionRepository;

        public CompetitionService(IRepository<Competition> competitionRepository)
        {
            _competitionRepository = competitionRepository
                ?? throw new ArgumentNullException(nameof(competitionRepository));
        }

        public void Add(Competition competition)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));
            if (competition.MatchDay == null)
                competition.MatchDay = new Dictionary<int, DateTime>();

            _competitionRepository.Add(competition);
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
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.");

            _competitionRepository.Delete(id);
        }

        public IList<Competition> GetCompetitions()
        {
            return _competitionRepository.Load();
        }

        public Competition? GetCompetitionById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _competitionRepository
                .Load()
                .FirstOrDefault(c => c.Id == id);
        }

        public void SaveAll(IList<Competition> competitions)
        {
            _competitionRepository.Create(competitions, true);
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
    }
}
