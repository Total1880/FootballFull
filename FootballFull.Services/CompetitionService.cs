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
            if (competition.MatchDayPerWeek == null)
                competition.MatchDayPerWeek = new Dictionary<int, int>();

            _competitionRepository.Add(competition);
        }

        public void Update(Competition competition)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));
            if (competition.Id == Guid.Empty)
                throw new ArgumentException("Competition must have a valid Id.");
            if (competition.MatchDayPerWeek == null)
                competition.MatchDayPerWeek = new Dictionary<int, int>();

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
                throw new ArgumentException("Id cannot be empty.");

            return _competitionRepository
                .Load()
                .FirstOrDefault(c => c.Id == id);
        }

        public void SaveAll(IList<Competition> competitions)
        {
            _competitionRepository.Create(competitions, true);
        }

        private void GenerateMatchDays(Competition competition)
        {
            if (competition.MatchDayPerWeek == null)
                competition.MatchDayPerWeek = new Dictionary<int, int>();
        }

        public IDictionary<int, int> UpdateMatchDaysPerWeek(Guid competitionId, IDictionary<int, int> matchDaysPerWeek)
        {
            var competition = GetCompetitionById(competitionId);
            if (competition == null)
                throw new ArgumentException("Competition not found.");
            if (competition.MatchDayPerWeek == null)
                competition.MatchDayPerWeek = new Dictionary<int, int>();
            foreach (var kvp in matchDaysPerWeek)
            {
                if (competition.MatchDayPerWeek.ContainsKey(kvp.Key))
                    competition.MatchDayPerWeek[kvp.Key] = kvp.Value;
                else
                    competition.MatchDayPerWeek.Add(kvp.Key, kvp.Value);
            }
            Update(competition);

            return competition.MatchDayPerWeek;
        }
    }
}
