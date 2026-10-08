using FootballFull.Models;
using FootballFull.Services.Interfaces;
using FootballFull.Services.UI;
using OlavFramework;

namespace FootballFull.Services
{
    public sealed record GameInitializationResult(
        Guid UserCountryId,
        int Year,
        DateTime CurrentDate,
        DateTime NewSeasonDate,
        IList<ClubPerCompetition> ClubsPerCompetition,
        IList<Competition> Competitions,
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
            _gameUI = gameUI;
        }

        public GameInitializationResult Initialize(bool isNew)
        {
            var userCountryId = ChoosePlayerCompetition();

            if (isNew)
            {
                ResetStrength();
                CreateTrainers();
                _competitionService.InitializeStarterCompetition(userCountryId);
            }

            var clubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            var competitions = _competitionService.GetCompetitions();
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
                clubsPerCompetition, competitions, fixtures, cupFixtures,
                internationalFixtures, associations);
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
