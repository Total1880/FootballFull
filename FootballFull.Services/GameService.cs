using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Services.Interfaces;
using FootballFull.Services.UI;
using FootballFull.Services.UI.ViewModels;
using OlavFramework;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class GameService : IGameService
    {
        private readonly ISeasonService _seasonService;
        private readonly IFixtureService _fixtureService;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly ICountryService _countryService;
        private readonly ITrainerService _trainerService;
        private readonly ISeasonFinancialResultService _seasonFinancialResultService;
        private readonly IEndOfSeasonService _endOfSeasonService;
        private readonly ISeasonEventService _seasonEventService;
        private readonly IStrengthService _strengthService;
        private readonly IFootballAssociationsService _footballAssociationsService;
        private readonly IGameUI _gameUI;

        private IList<ClubPerCompetition> _clubsPerCompetition = new List<ClubPerCompetition>();
        private IList<Competition> _competitions = new List<Competition>();
        private IList<Fixture> _fixtures = new List<Fixture>();
        private IList<Fixture> _cupFixtures = new List<Fixture>();
        private IList<Trainer> _trainers;
        private IList<Fixture>? _internationalFixtures;
        private IList<FootballAssociation> _footballAssociations = new List<FootballAssociation>();

        private Guid _userCountryId;
        private DateTime _currentDate;
        private DateTime _newSeasonDate;
        private int _year;

        public GameService(
            ISeasonService seasonService,
            IFixtureService fixtureService,
            IClubService clubService,
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            ICountryService countryService,
            ITrainerService trainerService,
            IEndOfSeasonService endOfSeasonService,
            ISeasonFinancialResultService seasonFinancialResultService,
            ISeasonEventService seasonEventService,
            IStrengthService strengthService,
            IFootballAssociationsService footballAssociationsService,
            IGameUI gameUI)
        {
            _seasonService = seasonService;
            _fixtureService = fixtureService;
            _clubService = clubService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _countryService = countryService;
            _endOfSeasonService = endOfSeasonService;
            _seasonFinancialResultService = seasonFinancialResultService;
            _seasonEventService = seasonEventService;
            _strengthService = strengthService;
            _trainerService = trainerService;
            _footballAssociationsService = footballAssociationsService;
            _gameUI = gameUI;

            _trainers = _trainerService.Load();
        }

        public void Run(bool isNew)
        {
            // User club kiezen
            _userCountryId = ChoosePlayerCompetition();

            // Initialize data
            if (isNew)
            {
                ResetStrength();
                CreateTrainers();
                _internationalFixtures = null;
                _competitionService.InitializeStarterCompetition(_userCountryId);
            }

            // Data laden
            _clubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            _competitions = _competitionService.GetCompetitions();
            CreateFootballAssocations();

            // Eerste seizoen initialiseren
            _seasonService.Initialize(_clubsPerCompetition);
            _year = _seasonService.Year > 0
                ? _seasonService.Year
                : DateTime.Now.Year;
            _seasonService.Year = _year;
            _currentDate = new DateTime(_year, 7, 1);
            _newSeasonDate = _currentDate.AddYears(1);
            _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
            _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);

            if (!isNew)
            {
                _internationalFixtures = _seasonService.InitializeInternationalGames(_currentDate, true);
            }

            GameLoop();
        }

        private void GameLoop()
        {
            while (true)
            {
                var dashboard = CreateDashboard();

                var choice = _gameUI.ShowMainMenu(dashboard);

                switch (choice)
                {
                    case MainMenuChoice.Continue:
                        var gamesToShow = PlayUntilNextMatchday();

                        _gameUI.ShowTable(CreateTableDashBoard(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id));
                        if (gamesToShow != null && gamesToShow.LeagueCompetition)
                            _gameUI.ShowResults(GetResult(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id, _currentDate));
                        _gameUI.ShowFixtures(GetNextFixture(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id, _currentDate), true);

                        if (gamesToShow != null && gamesToShow.CupCompetition)
                            _gameUI.ShowResults(GetResult(_competitions.First(_ => _.CountryId == _userCountryId && _.Type == CompetitionType.Cup).Id, _currentDate), true);
                        if (gamesToShow != null && gamesToShow.InternationalCompetition)
                            _gameUI.ShowResults(GetResult(_competitions.First(_ => _.Type == CompetitionType.International).Id, _currentDate), true);
                        break;
                    case MainMenuChoice.ShowOtherCompetitions:
                        _gameUI.ShowTable(CreateTableDashBoard(_gameUI.ChooseCompetitions(_competitions.Where(_ => _.Id != _competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id).ToList()).Id), true);
                        break;

                    case MainMenuChoice.Save:
                        _seasonService.SaveGame();
                        _gameUI.ShowMessage("Spel opgeslagen", "Je spel werd succesvol opgeslagen.");
                        break;

                    case MainMenuChoice.SaveAndExit:
                        _seasonService.SaveGame();
                        return;
                }
            }
        }

        private CompetitionTableViewModel CreateTableDashBoard(Guid competitionId)
        {
            var ranking = _seasonService.GetRanking(competitionId);

            foreach (var rank in ranking)
                if (rank.Club == null) rank.Club = _clubService.GetClubById(rank.ClubId);

            return new CompetitionTableViewModel
            {
                CompetitionName = _competitions.First(_ => _.Id == competitionId).Name,
                ClubLeagueCompetitions = ranking
            };
        }

        private GameDashboardViewModel CreateDashboard()
        {
            var association = _footballAssociations.Single(fa => fa.CountryId == _userCountryId);
            var competitionIds = _competitions
                .Where(c => c.CountryId == _userCountryId && c.Type == CompetitionType.League)
                .Select(c => c.Id)
                .ToHashSet();
            var nextMatchday = GetNextMatchday();

            return new GameDashboardViewModel
            {
                AssociationName = association.Name,
                SeasonStartYear = _year,
                CurrentDate = _currentDate,
                Balance = association.Balance,
                Reputation = association.Reputation,
                ReputationDescription = association.ReputationDescription,
                ClubCount = _clubPerCompetitionService
                    .GetAllClubPerCompetitionForCountry(_userCountryId).Count,
                CompetitionCount = competitionIds.Count,
                NextMatchday = nextMatchday,
                NextMatchCount = nextMatchday.HasValue ? CountMatchesOn(nextMatchday.Value) : 0
            };
        }

        private WeekGamesToShow PlayUntilNextMatchday()
        {
            var nextMatchday = GetNextMatchday();
            var weekGamesToShow = new WeekGamesToShow();

            if (!nextMatchday.HasValue || nextMatchday.Value >= _newSeasonDate)
            {
                FinishSeason();
                return null;
            }

            while (_currentDate <= nextMatchday.Value)
            {
                if (_seasonService.PlayMatchDay(_fixtures, _currentDate, false, _userCountryId))
                    weekGamesToShow.LeagueCompetition = true;

                if (PlayCupGames(_currentDate))
                    weekGamesToShow.CupCompetition = true;

                if (PlayInternationalGames(_currentDate))
                    weekGamesToShow.InternationalCompetition = true;

                _seasonService.UpdateWeekStats(_userCountryId, _currentDate);
                _currentDate = _currentDate.AddDays(1);
            }

            return weekGamesToShow;
        }

        private DateTime? GetNextMatchday()
        {
            var competitionIds = _competitions
                .Where(c => c.CountryId == _userCountryId || c.Type == CompetitionType.International)
                .Select(c => c.Id)
                .ToHashSet();

            return _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures ?? Array.Empty<Fixture>())
                .Where(f => f.MatchDay >= _currentDate)
                .Where(f => competitionIds.Contains(f.CompetitionId) ||
                            f.HomeTeam?.CountryId == _userCountryId ||
                            f.AwayTeam?.CountryId == _userCountryId)
                .Select(f => f.MatchDay)
                .OrderBy(date => date)
                .Cast<DateTime?>()
                .FirstOrDefault();
        }

        private int CountMatchesOn(DateTime date)
        {
            var competitionIds = _competitions
                .Where(c => c.CountryId == _userCountryId)
                .Select(c => c.Id)
                .ToHashSet();

            return _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures ?? Array.Empty<Fixture>())
                .Count(f => f.MatchDay == date &&
                    (competitionIds.Contains(f.CompetitionId) ||
                     f.HomeTeam?.CountryId == _userCountryId ||
                     f.AwayTeam?.CountryId == _userCountryId));
        }

        private void FinishSeason()
        {
            _gameUI.ShowMessage("Seizoen afgelopen", $"Seizoen {_year}/{_year + 1} is afgelopen.");

            _internationalFixtures = _seasonService.InitializeInternationalGames(_newSeasonDate);
            CalculateSeasonFinancialResult();
            CalculateClubFinancialResults();
            _strengthService.RecalculateClubStrengths();
            _strengthService.RecalculateCompetitionStrengths(_seasonService.Year);
            Events();
            EndOfSeasonChoices();

            _year++;
            _seasonService.Year = _year;
            _currentDate = new DateTime(_year, 7, 1);
            _newSeasonDate = _currentDate.AddYears(1);
            _clubsPerCompetition = _seasonService.InitializeNewSeason(_year);
            _competitions = _competitionService.GetCompetitions();
            _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
            _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);
            _seasonService.SaveGame();
        }

        private void CreateFootballAssocations()
        {
            _footballAssociations = _footballAssociationsService.GetAll() ?? new List<FootballAssociation>();
            var countries = _countryService.GetCountries();

            foreach (var country in countries)
            {
                if (_footballAssociations.Any(_ => _.CountryId == country.Id)) continue;
                var newFA = new FootballAssociation
                {
                    Id = Guid.NewGuid(),
                    CountryId = country.Id,
                    Name = country.Name + " FA",
                    Balance = Configuration.StartBalance,
                    Reputation = Configuration.StartReputation
                };
                _footballAssociationsService.Add(newFA);
                _footballAssociations.Add(newFA);
            }
        }

        private void Events()
        {
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);

            var events = _seasonEventService.GetRandomSeasonEvents();
            Console.Clear();
            Console.WriteLine(events.Description);
            Console.WriteLine($"Balance Change: {events.BalanceChange}");
            Console.WriteLine($"Reputation Change: {events.ReputationChange}");
            Console.ReadLine();

            footballAssociation.Balance += events.BalanceChange;
            footballAssociation.Reputation += events.ReputationChange;

            _footballAssociationsService.Update(footballAssociation);
        }

        private void CalculateSeasonFinancialResult()
        {
            var existingClubs = _clubPerCompetitionService.GetAllClubPerCompetitionForCountry(_userCountryId);
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);

            var seasonFinancialResult = _seasonFinancialResultService.CalculateFinancialResults(existingClubs.Count, _competitions.Where(c => c.CountryId == _userCountryId).Count(), footballAssociation);

            ShowSeasonFinancialResult(seasonFinancialResult, footballAssociation);

            _footballAssociationsService.Update(footballAssociation);
        }

        private void CalculateClubFinancialResults()
        {
            foreach (var country in _countryService.GetCountries())
            {
                _endOfSeasonService.ProcessClubFinances(country.Id);
            }
        }

        private void ShowSeasonFinancialResult(SeasonFinancialResult seasonFinancialResult, FootballAssociation footballAssociation)
        {
            Console.Clear();
            Console.WriteLine($"=== Season Financial Result for {footballAssociation.Name} ===");
            Console.WriteLine($"Club Income: {seasonFinancialResult.ClubIncome:C}");
            Console.WriteLine($"Reputation Income: {seasonFinancialResult.ReputationIncome:C}");
            Console.WriteLine($"Bonus Income: {seasonFinancialResult.BonusIncome:C}");
            Console.WriteLine();
            Console.WriteLine($"Club Costs: {seasonFinancialResult.ClubCosts:C}");
            Console.WriteLine($"Competition Costs: {seasonFinancialResult.CompetitionCosts:C}");
            Console.WriteLine($"Organisation Costs: {seasonFinancialResult.OrganisationCosts:C}");
            Console.WriteLine();
            Console.WriteLine($"Net Result: {seasonFinancialResult.NetResult:C}");
            Console.WriteLine($" Balance: {footballAssociation.Balance:C}");
            Console.WriteLine();
            Console.WriteLine($"Reputation Change: {seasonFinancialResult.ReputationChange}");
            Console.WriteLine($"Reputation: {footballAssociation.Reputation} ({footballAssociation.ReputationDescription})");
            Console.ReadLine();
        }

        private void EndOfSeasonChoices()
        {
            var footballAssociation = _footballAssociations
                .First(fa => fa.CountryId == _userCountryId);

            var options = _endOfSeasonService.GetOptions(
                _userCountryId,
                footballAssociation);

            if (!options.CanAddClub && !options.CanCreateLowerDivision)
            {
                ShowMessage(options.CannotAddClubReason);
                return;
            }

            if (options.CanCreateLowerDivision &&
                AskToCreateLowerDivision())
            {
                var numberOfClubs = AskNumberOfClubs(options.CurrentClubCount);

                _endOfSeasonService.CreateLowerDivision(
                    _userCountryId,
                    numberOfClubs,
                    footballAssociation);

                _competitions = _competitionService.GetCompetitions();

                return;
            }

            if (!options.CanAddClub)
            {
                ShowMessage(options.CannotAddClubReason);
                return;
            }

            Console.WriteLine("Wil je een nieuwe club toevoegen? Kostprijs {0} (y/n)", Configuration.NewClubCost);
            var input = Console.ReadLine();
            if (input?.ToLower() != "y")
            {
                return;
            }

            var applicants = _endOfSeasonService.GetApplicantClubs(
                _userCountryId,
                footballAssociation,
                3);

            while (applicants.Count < 3)
            {
                Console.Clear();
                Console.WriteLine(
                    $"Er zijn nog {3 - applicants.Count} kandidaat-club(s) nodig.");

                Console.Write("Geef de naam van de nieuwe club: ");
                var clubName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(clubName))
                {
                    ShowMessage("De naam van een club mag niet leeg zijn.");
                    continue;
                }

                var newClub = _endOfSeasonService.CreateApplicantClub(
                    _userCountryId,
                    clubName);

                applicants.Add(newClub);
            }

            var selectedClub = AskPlayerToSelectClub(applicants);

            _endOfSeasonService.AdmitClub(
                _userCountryId,
                selectedClub.Id,
                footballAssociation);
        }

        private Club AskPlayerToSelectClub(IList<Club> applicants)
        {
            if (applicants == null || applicants.Count == 0)
                throw new ArgumentException(
                    "Er zijn geen kandidaat-clubs beschikbaar.",
                    nameof(applicants));

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Aanvragen van clubs ===");
                Console.WriteLine();
                Console.WriteLine(
                    "De volgende clubs willen toetreden tot de competitie:");
                Console.WriteLine();

                for (var i = 0; i < applicants.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {applicants[i].Name}");
                }

                Console.WriteLine();
                Console.Write(
                    $"Kies een club (1-{applicants.Count}): ");

                var input = Console.ReadLine();

                if (int.TryParse(input, out var selectedNumber) &&
                    selectedNumber >= 1 &&
                    selectedNumber <= applicants.Count)
                {
                    return applicants[selectedNumber - 1];
                }

                Console.WriteLine();
                Console.WriteLine(
                    "Ongeldige keuze. Kies een nummer uit de lijst.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }
        }

        private int AskNumberOfClubs(int currentClubCount)
        {
            const int minimumClubsPerDivision = 2;

            var minimumClubsToMove = minimumClubsPerDivision;
            var maximumClubsToMove =
                currentClubCount - minimumClubsPerDivision;

            if (maximumClubsToMove < minimumClubsToMove)
            {
                throw new InvalidOperationException(
                    "Er zijn onvoldoende clubs om twee geldige divisies te maken.");
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Lagere divisie oprichten ===");
                Console.WriteLine();
                Console.WriteLine(
                    $"Er zijn momenteel {currentClubCount} clubs.");
                Console.WriteLine(
                    "De laagst geklasseerde clubs worden naar Division 2 verplaatst.");
                Console.WriteLine();
                Console.WriteLine(
                    $"Je kan tussen {minimumClubsToMove} en " +
                    $"{maximumClubsToMove} clubs verplaatsen.");
                Console.WriteLine();

                Console.Write("Hoeveel clubs wil je verplaatsen? ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out var numberOfClubs) &&
                    numberOfClubs >= minimumClubsToMove &&
                    numberOfClubs <= maximumClubsToMove)
                {
                    return numberOfClubs;
                }

                Console.WriteLine();
                Console.WriteLine(
                    $"Voer een getal in tussen {minimumClubsToMove} " +
                    $"en {maximumClubsToMove}.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }

        }

        private bool AskToCreateLowerDivision()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Einde van het seizoen ===");
                Console.WriteLine();
                Console.WriteLine("Je kan dit seizoen:");
                Console.WriteLine();
                Console.WriteLine("[E] Een extra club toelaten");
                Console.WriteLine("[L] Een lagere divisie oprichten");
                Console.WriteLine();
                Console.Write("Maak een keuze: ");

                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.E:
                        return false;

                    case ConsoleKey.L:
                        return true;

                    default:
                        Console.WriteLine();
                        Console.WriteLine(
                            "Ongeldige keuze. Kies E of L.");
                        Console.WriteLine(
                            "Druk op een toets om opnieuw te proberen...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private void ShowMessage(string? message)
        {
            Console.Clear();
            Console.WriteLine("=== Einde van het seizoen ===");
            Console.WriteLine();

            Console.WriteLine(
                string.IsNullOrWhiteSpace(message)
                    ? "Deze actie is momenteel niet beschikbaar."
                    : message);

            Console.WriteLine();
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
        }

        private void ShowNews(DateTime date, Guid competitionId)
        {
            Console.Clear();
            var countryId = _userCountryId;

            // Eén query, maar we vermijden dubbele enumeratie door te materializen als nodig
            var matches = _seasonService.NewsMessages.Where(nm =>
                nm.CountryId == countryId &&
                nm.CompetitionId == competitionId &&
                nm.Date == date);

            using var enumerator = matches.GetEnumerator();
            if (!enumerator.MoveNext())
                return; // geen nieuws -> meteen klaar (scheelt ook een ReadKey)

            // eerste item is er al
            do
            {
                Console.WriteLine(enumerator.Current.Message);
            }
            while (enumerator.MoveNext());

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey(true);
        }


        #region Helpers

        private void ShowBetweenMatchdaysMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Volgende speeldag");
                Console.WriteLine("2. Andere lopende competities bekijken");
                Console.WriteLine("3. Club bekijken");
                Console.WriteLine("4. Trainers bekijken");
                Console.WriteLine("5. Internationale ranking bekijken");
                Console.WriteLine("0. Stoppen");
                Console.Write("Maak een keuze: ");

                var input = Console.ReadKey(true);
                Console.WriteLine(input.KeyChar);

                switch (input.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        return;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        ShowOtherCompetitionsMenu();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        ClubMenu();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        Console.WriteLine("Deze functie is nog niet beschikbaar.");
                        Console.WriteLine("Druk op een toets om terug te gaan...");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        DisplayInternationalRankingPerYear();
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        Environment.Exit(0);
                        return;

                    default:
                        Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
                        Thread.Sleep(750);
                        break;
                }
            }
        }

        private void ClubMenu()
        {
            Console.Clear();
            Console.WriteLine();
            var trainer = _seasonService.UserTrainer(_userCountryId);
            Console.WriteLine($"Trainer: {trainer?.Name} {trainer?.LastName}");
#if DEBUG
            Console.WriteLine($"Tactical: {trainer?.TacticalSkill}");
            Console.WriteLine($"Motivational: {trainer?.Motivation}");
#endif
            Console.WriteLine();
            Console.WriteLine("Wil je de trainer ontslaan? (J/N)");
            var input = Console.ReadKey(true);
            switch (input.Key)
            {
                case ConsoleKey.J:
                case ConsoleKey.Y:
                    _seasonService.NewTrainer(_userCountryId, _currentDate);
                    Console.WriteLine("Er werd een nieuwe trainer aangesteld.");
                    Console.WriteLine("Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
                default:
                    break;
            }

        }

        private bool PlayCupGames(DateTime date)
        {
            if (_cupFixtures == null || !_cupFixtures.Any())
                return false;
            if (!_cupFixtures.Any(_ => _.MatchDay == date))
                return false;

            var cupCompetitions = _competitions
                .Where(_ => _.Type == Competition.CompetitionType.Cup)
                .ToList();

            foreach (var cupCompetition in cupCompetitions)
            {
                var fixturesForCompetition = _cupFixtures
                    .Where(_ => _.CompetitionId == cupCompetition.Id && _.MatchDay == date)
                    .ToList();

                if (fixturesForCompetition.Count == 0)
                    continue;

                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId == Guid.Empty || fixture.AwayTeamId == Guid.Empty)
                        continue;


                    var homeTier = GetClubTier(fixture.HomeTeamId);
                    var awayTier = GetClubTier(fixture.AwayTeamId);
                }

                // Speel enkel deze ronde
                _seasonService.PlayMatchDay(fixturesForCompetition, date, true, _userCountryId, true);

                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId != Guid.Empty && fixture.AwayTeamId != Guid.Empty)
                    {

                        if (fixture.HomeTeamId == _userCountryId || fixture.AwayTeamId == _userCountryId)
                            Console.ForegroundColor = ConsoleColor.Yellow;

                        var homeTier = GetClubTier(fixture.HomeTeamId);
                        var awayTier = GetClubTier(fixture.AwayTeamId);

                    }
                    // winners voor volgende ronde
                    var winner = fixture.HomeScore > fixture.AwayScore ? fixture.HomeTeam : fixture.AwayTeam;
                    var cupNextFixtures = _cupFixtures.FirstOrDefault(_ => _.CupPreviousFixtureHomeTeam == fixture);

                    if (cupNextFixtures != null)
                    {
                        if (cupNextFixtures.HomeTeamId == Guid.Empty)
                        {
                            cupNextFixtures.HomeTeam = winner;
                            cupNextFixtures.HomeTeamId = winner.Id;
                        }
                    }
                    else
                    {
                        cupNextFixtures = _cupFixtures.FirstOrDefault(_ => _.CupPreviousFixtureAwayTeam == fixture);
                        if (cupNextFixtures != null && cupNextFixtures.AwayTeamId == Guid.Empty)
                        {
                            cupNextFixtures.AwayTeam = winner;
                            cupNextFixtures.AwayTeamId = winner.Id;
                        }
                    }
                }
            }
            return true;
        }

        private bool PlayInternationalGames(DateTime date)
        {
            if (_internationalFixtures == null || !_internationalFixtures.Any())
                return false;
            if (!_internationalFixtures.Any(_ => _.MatchDay == date))
                return false;

            var fixturesForRound = _internationalFixtures
                .Where(_ => _.MatchDay == date)
                .ToList();

            if (fixturesForRound.Count == 0)
                return false;

            foreach (var fixture in fixturesForRound)
            {
                if (fixture.HomeTeamId == Guid.Empty || fixture.AwayTeamId == Guid.Empty)
                    continue;

                var homeName =
                    fixture.HomeTeam != null ? fixture.HomeTeam.Name :
                    fixture.CupPreviousFixtureHomeTeam != null ? "Winner previous match" :
                    "TBD";

                var awayName =
                    fixture.AwayTeam != null ? fixture.AwayTeam.Name :
                    fixture.CupPreviousFixtureAwayTeam != null ? "Winner previous match" :
                    "TBD";
            }

            _seasonService.PlayMatchDay(fixturesForRound, date, true, _userCountryId, true);

            foreach (var fixture in fixturesForRound)
            {
                var winner = fixture.HomeScore > fixture.AwayScore ? fixture.HomeTeam : fixture.AwayTeam;
                var cupNextFixtures = _internationalFixtures.FirstOrDefault(_ => _.CupPreviousFixtureHomeTeam == fixture);

                if (cupNextFixtures != null)
                {
                    if (cupNextFixtures.HomeTeamId == Guid.Empty)
                    {
                        cupNextFixtures.HomeTeam = winner;
                        cupNextFixtures.HomeTeamId = winner.Id;
                    }
                }
                else
                {
                    cupNextFixtures = _internationalFixtures.FirstOrDefault(_ => _.CupPreviousFixtureAwayTeam == fixture);
                    if (cupNextFixtures != null && cupNextFixtures.AwayTeamId == Guid.Empty)
                    {
                        cupNextFixtures.AwayTeam = winner;
                        cupNextFixtures.AwayTeamId = winner.Id;
                    }
                }
            }

            return true;
        }

        private List<Fixture> GetResult(Guid competitionId, DateTime date)
        {
            var lastDate = _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures)
    .Where(fixture =>
        fixture.CompetitionId == competitionId &&
        fixture.MatchDay <= date)
    .Select(fixture => fixture.MatchDay)
    .OrderByDescending(date => date)
    .FirstOrDefault();

            return _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures)
                .Where(_ => _.MatchDay == lastDate && _.CompetitionId == competitionId)
                .ToList();
        }

        private void DisplayInternationalRankingPerYear()
        {
            var years = Enumerable.Range(_year - 4, 5);

            var rankings = _seasonService.ClubInternationalRankings
                .GroupBy(c => c.CountryId)
                .Select(g =>
                {
                    var ranking = new CountryCoefficientRanking
                    {
                        CountryId = g.Key,
                        Country = g.First().Country
                    };

                    foreach (var y in years)
                    {
                        // Ruwe punten per jaar
                        var totalPointsThisYear = g.Sum(c =>
                            c.PointsPerYear.TryGetValue(y, out var pts) ? pts : 0);

                        ranking.RawPointsPerYear[y] = totalPointsThisYear;

                        // Clubs die punten hebben in dat jaar
                        var clubsThisYear = g.Count(c =>
                            c.PointsPerYear.ContainsKey(y));

                        ranking.ClubsParticipatingPerYear[y] = clubsThisYear;

                        // Coefficient voor dit jaar
                        var yearlyCoefficient =
                            clubsThisYear == 0 ? 0.0 :
                            (double)totalPointsThisYear / clubsThisYear;

                        ranking.CoefficientPerYear[y] = yearlyCoefficient;
                    }

                    ranking.FiveYearCoefficient =
                        ranking.CoefficientPerYear.Values.Sum();

                    return ranking;
                })
                .OrderByDescending(r => r.FiveYearCoefficient)
                .ToList();

            DisplayCountryCoefficientRanking(rankings);
        }

        private void DisplayCountryCoefficientRanking(
    IList<CountryCoefficientRanking> rankings)
        {
            if (rankings == null || !rankings.Any())
            {
                Console.WriteLine("Geen landencoëfficiënten beschikbaar.");
                return;
            }

            // Zorg dat de lijst gesorteerd is (hoogste eerst)
            rankings = rankings
                .OrderByDescending(r => r.FiveYearCoefficient)
                .ToList();

            var years = Enumerable.Range(_year - 4, 5).ToList();

            Console.WriteLine("=== Country Coefficient Ranking (5-jaars) ===");
            Console.WriteLine();

            int position = 1;
            foreach (var r in rankings)
            {
                var countryName = r.Country?.Name ?? r.CountryId.ToString();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    $"{position,2}. {countryName} – {r.FiveYearCoefficient:F2} punten");
                Console.ResetColor();

                // Detail per jaar
                foreach (var year in years.OrderByDescending(y => y))
                {
                    r.CoefficientPerYear.TryGetValue(year, out var coeff);
                    r.ClubsParticipatingPerYear.TryGetValue(year, out var clubs);
                    r.RawPointsPerYear.TryGetValue(year, out var rawPoints);

                    // bv: 2025: 7.50 (3 clubs, 22 punten)
                    Console.WriteLine(
                        $"    {year}: {coeff,6:F2} " +
                        $"({clubs} clubs, {rawPoints} punten)");
                }

                Console.WriteLine();
                position++;
            }

            Console.ReadKey();
        }

        private List<Fixture> GetNextFixture(Guid competitionId, DateTime fromDate)
        {
            var nextDate = _fixtures
                .Where(fixture =>
                    fixture.CompetitionId == competitionId &&
                    fixture.MatchDay >= fromDate &&
                    (fixture.AwayTeam.CountryId == _userCountryId || fixture.HomeTeam.CountryId == _userCountryId))
                .Select(fixture => fixture.MatchDay)
                .OrderBy(date => date)
                .FirstOrDefault();

            if (nextDate == default || nextDate >= _newSeasonDate)
                return null;

            var fixtures = _fixtures.Where(f =>
                f.CompetitionId == competitionId &&
                f.MatchDay == nextDate &&
                (f.AwayTeam.CountryId == _userCountryId || f.HomeTeam.CountryId == _userCountryId));

            return fixtures.ToList();
        }

        private void ResetStrength()
        {
            var countries = _countryService.GetCountries();
            var clubs = _clubService.GetClubs();

            foreach (var country in countries)
            {
                //ResetClubStrength(clubs, country);
                ResetCompetitionStrength(country);
            }
        }

        private bool ResetCompetitionStrength(Country country)
        {
            var competitionsInCountry = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == country.Id && c.Type == Competition.CompetitionType.League)
                .OrderBy(c => c.Tier)
                .ToList();

            if (!competitionsInCountry.Any())
                return false;

            var current = Configuration.MaxStrength - Configuration.MinStrength;
            var counter = competitionsInCountry.Count / (current == 0 ? 1 : current);
            var step = current / (competitionsInCountry.Count == 0 ? 1 : competitionsInCountry.Count);

            foreach (var competition in competitionsInCountry)
            {
                competition.Strength = current;
                _competitionService.Update(competition);
                current -= step;
            }
            return true;
        }

        private int GetClubTier(Guid clubId)
        {
            // Zoek in welke league-competitie deze club speelt
            var clubInCompetition = _clubsPerCompetition
                .FirstOrDefault(c => c.ClubId == clubId &&
                                     _competitions.Any(comp => comp.Id == c.CompetitionId && comp.Type == Competition.CompetitionType.League));
            if (clubInCompetition == null)
                return 0; // of een default/unknown waarde

            var leagueCompetition = _competitions.FirstOrDefault(c => c.Id == clubInCompetition.CompetitionId);
            if (leagueCompetition == null)
                return 0;

            return leagueCompetition.Tier;
        }

        private void ShowOtherCompetitionsMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Other Competitions ===");
            Console.WriteLine();

            var competitions = _competitionService.GetCompetitions()
                .Where(_ => _.Type == Competition.CompetitionType.League)
                .OrderBy(_ => _.CountryId)
                .ThenBy(_ => _.Tier)
                .ToList();

            for (int i = 0; i < competitions.Count; i++)
            {
                var comp = competitions[i];
                if (comp.Country == null)
                {
                    comp.Country = _countryService.GetCountryById(comp.CountryId);
                }
                Console.WriteLine($"{i + 1}. {comp.Country.Name} – {comp.Name} (Tier {comp.Tier})");
            }

            Console.WriteLine("0. Terug");
            Console.Write("Maak een keuze: ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice == 0)
                return;

            if (choice > 0 && choice <= competitions.Count)
            {
                var selected = competitions[choice - 1];
                ShowCompetitionDetailMenu(selected);
            }
        }

        private void ShowCompetitionDetailMenu(Competition competition)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {competition.Country.Name} – {competition.Name} ===");
                Console.WriteLine("1. Stand bekijken");
                Console.WriteLine("2. Fixtures bekijken");
                Console.WriteLine("3. Resultaten tot nu toe");
                Console.WriteLine("0. Terug");
                Console.Write("Maak een keuze: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        // Verplaatst naar viewmodel
                        break;
                    case "2":
                        DisplayFixturesForCompetition(competition.Id);
                        break;
                    case "3":
                        DisplayResultsForCompetition(competition.Id);
                        break;
                    case "0":
                        return;
                }

                Console.WriteLine("\nDruk op een toets om terug te gaan...");
                Console.ReadKey();
            }
        }

        private void DisplayFixturesForCompetition(Guid competitionId)
        {
            Console.Clear();
            Console.WriteLine("=== Fixtures ===");

            var fixtures = _fixtures
                .Where(_ => _.CompetitionId == competitionId)
                .OrderBy(_ => _.MatchDay)
                .ToList();

            foreach (var f in fixtures)
                Console.WriteLine($"MD {f.MatchDay}: {f.HomeTeam.Name} - {f.AwayTeam.Name}");
        }

        private void DisplayResultsForCompetition(Guid competitionId)
        {
            Console.Clear();
            Console.WriteLine("=== Results Played ===");

            var fixtures = _fixtures
                .Where(_ =>
                    _.CompetitionId == competitionId &&
                    _.MatchDay < _currentDate)
                .OrderBy(_ => _.MatchDay)
                .ToList();

            foreach (var f in fixtures)
                Console.WriteLine(
                    $"MD {f.MatchDay}: {f.HomeTeam.Name} {f.HomeScore} - {f.AwayScore} {f.AwayTeam.Name}");
        }

        private void CreateTrainers()
        {
            var clubIds = _clubService.GetClubs().Select(c => c.Id).ToList();

            foreach (var clubId in clubIds)
            {
                var existingTrainer = _trainerService.GetByClubId(clubId);
                if (existingTrainer == null)
                {
                    var newTrainer = _trainerService.CreateRandomTrainer(clubId);
                    _trainers.Add(newTrainer);

                }
            }

            _trainerService.SaveAll(_trainers);
        }

        private Guid ChoosePlayerCompetition()
        {
            var countries = _countryService.GetCountries();
            do
            {
                Console.Clear();
                Console.WriteLine("Kies het land of kies 0 voor een compleet nieuw land:");
                for (int i = 0; i < countries.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {countries[i].Name}");
                }
                Console.Write("\nGeef het nummer van het land: ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out int chosenIndex) && chosenIndex > 0 && chosenIndex <= countries.Count)
                {
                    var chosenCountry = countries[chosenIndex - 1].Id;
                    do
                    {
                        Console.Clear();
                        Console.WriteLine($"Je hebt gekozen: {countries[chosenIndex - 1].Name}");
                        return chosenCountry;
                    } while (true);
                }
                else if (chosenIndex == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Je hebt gekozen voor een compleet nieuw land.");
                    Console.Write("Geef de naam van het nieuwe land: ");
                    var newCountryName = Console.ReadLine();
                    _userCountryId = Guid.NewGuid();
                    _countryService.Add(new Country { Name = newCountryName, Id = _userCountryId });

                    CreateStarterClubs();

                    return _userCountryId;
                }
            } while (true);
        }

        private void CreateStarterClubs()
        {
            Console.WriteLine("Geef de namen van de starterclubs:");
            Console.Write("Club 1: ");
            var club1Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club1Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });
            Console.Write("Club 2: ");
            var club2Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club2Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });

            Console.Write("Club 3: ");
            var club3Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club3Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });

            Console.Write("Club 4: ");
            var club4Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club4Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });
            Console.Write("Club 5: ");
            var club5Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club5Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });

            Console.Write("Club 6: ");
            var club6Name = Console.ReadLine();
            _clubService.Add(new Club { Name = club6Name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });

        }
        #endregion
    }
}
