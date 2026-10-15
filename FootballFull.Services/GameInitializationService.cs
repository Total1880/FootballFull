using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Services.Interfaces;
using FootballFull.Services.UI;
using OlavFramework;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public sealed record GameInitializationResult(
        Guid UserCountryId,
        int Year,
        DateTime CurrentDate,
        DateTime NewSeasonDate,
        IList<ClubPerCompetition> ClubsPerCompetition,
        IList<Fixture> LeagueFixtures,
        IList<Fixture> CupFixtures,
        IList<Fixture>? InternationalFixtures,
        IList<FootballAssociation> FootballAssociations);

    public sealed class GameInitializationService : IGameInitializationService
    {
        private readonly ISeasonService _seasonService;
        private readonly IFixtureService _fixtureService;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly ICountryService _countryService;
        private readonly ITrainerService _trainerService;
        private readonly IFootballAssociationsService _footballAssociationsService;
        private readonly ICompetitionRulesService _competitionRulesService;
        private readonly IGameUI _gameUI;

        public GameInitializationService(
            ISeasonService seasonService,
            IFixtureService fixtureService,
            IClubService clubService,
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            ICountryService countryService,
            ITrainerService trainerService,
            IFootballAssociationsService footballAssociationsService,
            ICompetitionRulesService competitionRulesService,
            IGameUI gameUI)
        {
            _seasonService = seasonService;
            _fixtureService = fixtureService;
            _clubService = clubService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _countryService = countryService;
            _trainerService = trainerService;
            _footballAssociationsService = footballAssociationsService;
            _competitionRulesService = competitionRulesService;
            _gameUI = gameUI;
        }

        public GameInitializationResult Initialize(bool isNew)
        {
            var userCountryId = ChoosePlayerCompetition();

            if (isNew)
            {
                ResetStrength();
                CreateTrainers();
                InitializeStarterCompetition(userCountryId);
            }

            var clubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            var associations = EnsureFootballAssociations();

            _seasonService.Initialize(clubsPerCompetition);
            var year = _seasonService.Year > 0 ? _seasonService.Year : DateTime.Now.Year;
            _seasonService.Year = year;

            var currentDate = new DateTime(year, 7, 1);
            var nextSeasonDate = currentDate.AddYears(1);
            var fixtures = _fixtureService.Generate(clubsPerCompetition, currentDate);
            var cupFixtures = _seasonService.InitializeNationalCups(currentDate);
            var internationalFixtures = isNew
                ? null
                : _seasonService.InitializeInternationalGames(currentDate, true);

            return new GameInitializationResult(
                userCountryId, year, currentDate, nextSeasonDate,
                clubsPerCompetition, fixtures, cupFixtures,
                internationalFixtures, associations);
        }

        private void InitializeStarterCompetition(Guid userCountryId)
        {
            var competitions = _competitionService.GetCompetitionsForCountry(userCountryId, false);
            for (int i = 0; i < competitions.Count(); i++)
            {
                _clubPerCompetitionService.DeleteEverythingForCompetition(competitions[i].Id);
                _competitionRulesService.DeleteEverythingForCompetition(competitions[i].Id);
                _competitionService.Delete(competitions[i].Id);
            }

            var clubsWithFeeders = _clubService.GetClubs().Where(r => r.CountryId == userCountryId && r.FeederClubId != null && r.FeederClubId != Guid.Empty).ToList();
            foreach (var club in clubsWithFeeders)
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                _clubService.Delete((Guid)club.FeederClubId);
#pragma warning restore CS8629 // Nullable value type may be null.
                club.FeederClubId = null;
                _clubService.Update(club);
            }

            var newCompetition = new Competition
            {
                Id = Guid.NewGuid(),
                Name = "Division 1",
                CountryId = userCountryId,
                Tier = 1,
                Type = CompetitionType.League,
                Strength = new Random().Next(Configuration.MinStrength, Configuration.MinStrength + 3),
            };
            _competitionService.Add(newCompetition);

            var clubs = _clubService.GetClubs().Where(c => c.CountryId == userCountryId).OrderByDescending(_ => _.Strength).Take(6).ToList();

            foreach (var club in clubs)
            {
                _clubPerCompetitionService.AddClubToCompetition(club.Id, newCompetition.Id);

                club.Strength = new Random().Next(Configuration.MinStrength, Configuration.MinStrength + 3);
                _clubService.Update(club);
            }
        }

        private IList<FootballAssociation> EnsureFootballAssociations()
        {
            var associations = _footballAssociationsService.GetAll()
                ?? new List<FootballAssociation>();

            foreach (var country in _countryService.GetCountries())
            {
                if (associations.Any(fa => fa.CountryId == country.Id))
                    continue;

                var association = new FootballAssociation
                {
                    Id = Guid.NewGuid(),
                    CountryId = country.Id,
                    Name = country.Name + " FA",
                    Balance = Configuration.StartBalance,
                    Reputation = Configuration.StartReputation
                };
                _footballAssociationsService.Add(association);
                associations.Add(association);
            }

            return associations;
        }

        private void ResetStrength()
        {
            foreach (var country in _countryService.GetCountries())
                ResetCompetitionStrength(country);
        }

        private bool ResetCompetitionStrength(Country country)
        {
            var competitionsInCountry = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == country.Id &&
                            c.Type == Competition.CompetitionType.League)
                .OrderBy(c => c.Tier)
                .ToList();

            if (competitionsInCountry.Count == 0)
                return false;

            var current = Configuration.MaxStrength - Configuration.MinStrength;
            var step = current / competitionsInCountry.Count;
            foreach (var competition in competitionsInCountry)
            {
                competition.Strength = current;
                _competitionService.Update(competition);
                current -= step;
            }

            return true;
        }

        private void CreateTrainers()
        {
            var trainers = _trainerService.Load();
            foreach (var clubId in _clubService.GetClubs().Select(c => c.Id))
            {
                if (_trainerService.GetByClubId(clubId) != null)
                    continue;

                trainers.Add(_trainerService.CreateRandomTrainer(clubId));
            }

            _trainerService.SaveAll(trainers);
        }

        private Guid ChoosePlayerCompetition()
        {
            var countries = _countryService.GetCountries();
            while (true)
            {
                var chosenIndex = _gameUI.AskPlayerToSelectCountry(
                    GameUIMapper.Options(countries));

                if (chosenIndex > 0 && chosenIndex <= countries.Count)
                {
                    var country = countries[chosenIndex - 1];
                    _gameUI.ShowMessage("Keuze", $"Je hebt gekozen: {country.Name}");
                    return country.Id;
                }

                if (chosenIndex == 0)
                {
                    var newCountry = new Country
                    {
                        Id = Guid.NewGuid(),
                        Name = _gameUI.AskNewCountryName()
                    };

                    _countryService.Add(newCountry);
                    _gameUI.ShowMessage("Nieuw land aangemaakt",
                        $"Nieuw land '{newCountry.Name}' werd aangemaakt.");

                    CreateStarterClubs(newCountry.Id);
                    return newCountry.Id;
                }

                _gameUI.ShowMessage("Ongeldige keuze",
                    "Ongeldige keuze. Probeer opnieuw.");
            }
        }

        private void CreateStarterClubs(Guid countryId)
        {
            foreach (var name in _gameUI.AskStarterClubNames(6))
            {
                _clubService.Add(new Club
                {
                    Name = name,
                    CountryId = countryId,
                    Strength = new Random().Next(Configuration.MinStrength, 4)
                });
            }
        }
    }
}
