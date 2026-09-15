using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IRepository<Competition> _competitionRepository;
        private readonly IClubPerCompetitionRepository _clubPerCompetitionRepository;
        private readonly IRepository<CompetitionRules> _competitionRulesRepository;
        private readonly IRepository<Club> _clubRepository;

        public CompetitionService(IRepository<Competition> competitionRepository, IClubPerCompetitionRepository clubPerCompetitionRepository, IRepository<CompetitionRules> competitionRulesRepository, IRepository<Club> clubRepository)
        {
            _competitionRepository = competitionRepository
                ?? throw new ArgumentNullException(nameof(competitionRepository));
            _clubPerCompetitionRepository = clubPerCompetitionRepository
                ?? throw new ArgumentNullException(nameof(clubPerCompetitionRepository));
            _competitionRulesRepository = competitionRulesRepository
                ?? throw new ArgumentNullException(nameof(competitionRulesRepository));
            _clubRepository = clubRepository
                ?? throw new ArgumentNullException(nameof(clubRepository));
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

        public void InitializeStarterCompetition(Guid countryId)
        {
            var competitions = GetCompetitions().Where(c => c.CountryId == countryId).ToList();
            for (int i = 0; i < competitions.Count(); i++)
            {
                var clubPerCompetitionEntries = _clubPerCompetitionRepository.Load().Where(cpc => cpc.CompetitionId == competitions[i].Id).ToList();
                foreach (var entry in clubPerCompetitionEntries)
                    _clubPerCompetitionRepository.Delete(entry.ClubId, entry.CompetitionId);

                var rules = _competitionRulesRepository.Load().Where(r => r.CompetitionId == competitions[i].Id);
                foreach (var rule in rules)
                    _competitionRulesRepository.Delete(rule.Id);

                _competitionRepository.Delete(competitions[i].Id);
            }

            var clubsWithFeeders = _clubRepository.Load().Where(r => r.CountryId == countryId && r.FeederClubId != null && r.FeederClubId != Guid.Empty).ToList();
            foreach (var club in clubsWithFeeders)
            {
                _clubRepository.Delete((Guid)club.FeederClubId);
                club.FeederClubId = null;
                _clubRepository.Update(club);
            }

            var newCompetition = new Competition
            {
                Id = Guid.NewGuid(),
                Name = "Division 1",
                CountryId = countryId,
                Tier = 1,
                Type = CompetitionType.League,
                Strength = new Random().Next(OlavFramework.Configuration.MinStrength, OlavFramework.Configuration.MinStrength + 3),
            };
            _competitionRepository.Add(newCompetition);

            var clubs = _clubRepository.Load().Where(c => c.CountryId == countryId).OrderByDescending(_ => _.Strength).Take(6).ToList();

            foreach (var club in clubs)
            {
                _clubPerCompetitionRepository.Add(new ClubPerCompetition
                {
                    ClubId = club.Id,
                    CompetitionId = newCompetition.Id,
                });

                club.Strength = new Random().Next(OlavFramework.Configuration.MinStrength, OlavFramework.Configuration.MinStrength + 3);
                _clubRepository.Update(club);
            }
        }

        public IList<Competition> GetCompetitionsForCountry(Guid userCountryId)
        {
            return _competitionRepository.Load().Where(c => c.CountryId == userCountryId).ToList();
        }
    }
}
