using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Services.Interfaces;
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
        private readonly ICompetitionRulesService _competitionRulesService;

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
            ICompetitionRulesService competitionRulesService)
        {
            _seasonService = seasonService;
            _fixtureService = fixtureService;
            _clubService = clubService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _countryService = countryService;
            _trainerService = trainerService;
            _competitionRulesService = competitionRulesService;

            _trainers = _trainerService.Load();
        }

        public void Run(bool isNew)
        {
            var dayCounter = 0;

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

            //temp
            foreach (var country in _countryService.GetCountries())
            {
                _footballAssociations.Add(new FootballAssociation
                {
                    Id = Guid.NewGuid(),
                    CountryId = country.Id,
                    Name = $"{country.Name} Football Association",
                    Reputation = Configuration.StartReputation,
                    Balance = Configuration.StartBalance
                });
            }

            // Hoofdloop
            do
            {
                var competitionToShow = _competitions.OrderBy(c => c.Tier).First(_ => _.CountryId == _userCountryId);

                var fixturesLeft = true;
                if (competitionToShow == null)
                {
                    Console.WriteLine("De competitie van je club kon niet worden gevonden.");
                    Console.ReadKey(true);
                    return;
                }

                Console.Clear();

                // Fixture overview
                DisplayLeagueTable(competitionToShow.Id);
                Console.WriteLine();
                DisplayNextFixture(competitionToShow, _currentDate);

                Console.WriteLine();
                Console.WriteLine("Druk op een toets om het seizoen te starten...");
                Console.ReadKey(true);
                Console.Clear();

                do
                {
                    dayCounter++;

                    var userPlaysToday = UserPlaysOn(_currentDate);
                    if (fixturesLeft == true && (userPlaysToday || dayCounter >= 7))
                        ShowBetweenMatchdaysMenu();

                    Console.Clear();
                    Console.WriteLine($"=== Date {_currentDate:dddd dd/MM/yyyy} ===");

                    _seasonService.PlayMatchDay(_fixtures, _currentDate, false, _userCountryId);

                    DisplayLeagueTable(competitionToShow.Id);
                    Console.WriteLine();
                    DisplayResult(competitionToShow, _currentDate);
                    Console.WriteLine();
                    fixturesLeft = DisplayNextFixture(competitionToShow, _currentDate.AddDays(1));
                    if (fixturesLeft == true && (userPlaysToday || dayCounter >= 7))
                    {
                        dayCounter = 0;
                        Console.WriteLine();
                        Console.WriteLine("Druk op een toets om verder te gaan...");
                        Console.ReadKey(true);
                    }

                    PlayCupGames(_currentDate);
                    PlayInternationalGames(_currentDate);
                    _seasonService.UpdateWeekStats(_userCountryId, _currentDate);

                    Console.Clear();

                    ShowNews(_currentDate, competitionToShow.Id);
                    _currentDate = _currentDate.AddDays(1);
                } while (_currentDate < _newSeasonDate);

                // End of season
                Console.WriteLine($"Seizoen {_year}/{_year + 1} afgelopen.");
                Console.WriteLine("Druk op een toets om het volgende seizoen te starten...");
                Console.ReadKey(true);
                Console.Clear();

                // Internationale deelnemers worden nog bepaald met de eindstand
                // van het afgelopen seizoen.
                _internationalFixtures = _seasonService.InitializeInternationalGames(_currentDate);
                CalculateSeasonFinancialResult();
                EndOfSeasonChoices();

                _year++;
                _currentDate = new DateTime(_year, 7, 1);
                _newSeasonDate = _currentDate.AddYears(1);
                _clubsPerCompetition = _seasonService.InitializeNewSeason(_year);
                _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
                _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);

                _seasonService.SaveGame();

            } while (true);
        }

        private void CalculateSeasonFinancialResult()
        {
            var _seasonFinancialResult = new SeasonFinancialResult();
            var existingClubs = _clubPerCompetitionService.GetAllClubPerCompetitionForCountry(_userCountryId);
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);

            _seasonFinancialResult.ClubIncome = existingClubs.Count * Configuration.BasicClubRevenue;
            _seasonFinancialResult.ReputationIncome = footballAssociation.Reputation * Configuration.ReputationRevenueMultiplier;
            _seasonFinancialResult.BonusIncome = 0; // Placeholder for any bonus income logic

            _seasonFinancialResult.ClubCosts = existingClubs.Count * Configuration.BasicClubCost;
            _seasonFinancialResult.CompetitionCosts = _competitions.Where(c => c.CountryId == _userCountryId).Count() * Configuration.BasicCompetitionCost;
            _seasonFinancialResult.OrganisationCosts = Configuration.BasicOrganisationCost;

            _seasonFinancialResult.ReputationChange = +1;

            footballAssociation.Balance += _seasonFinancialResult.NetResult;
            footballAssociation.Reputation = footballAssociation.Reputation >= Configuration.MaxReputation ? 
                Configuration.MaxReputation : 
                footballAssociation.Reputation <= 1 ? 
                1 : footballAssociation.Reputation + _seasonFinancialResult.ReputationChange;

            ShowSeeasonFinancialResult(_seasonFinancialResult, footballAssociation);
        }

        private void ShowSeeasonFinancialResult(SeasonFinancialResult seasonFinancialResult, FootballAssociation footballAssociation)
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
            Console.WriteLine($"Reputation: {footballAssociation.Reputation}");
            Console.ReadLine();
        }

        private void EndOfSeasonChoices()
        {
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);
            var existingClubs = _clubPerCompetitionService.GetAllClubPerCompetitionForCountry(_userCountryId);
            var existingCompetitions = _competitionService.GetCompetitionsForCountry(_userCountryId);

            if(footballAssociation.Balance < 10000)
            {
                Console.Clear();
                Console.WriteLine("Je hebt onvoldoende saldo om een extra club toe te laten in de competitie.");
                Console.ReadLine();
                return;
            }

            if(footballAssociation.Reputation < 10 && existingClubs.Count >= 6)
            {
                Console.Clear();
                Console.WriteLine("Je reputatie is te laag om een extra club toe te laten in de competitie.");
                Console.ReadLine();
                return;
            }

            if (footballAssociation.Reputation < 15 && existingClubs.Count >= 7)
            {
                Console.Clear();
                Console.WriteLine("Je reputatie is te laag om een extra club toe te laten in de competitie.");
                Console.ReadLine();
                return;
            }


            if (existingClubs.Count >= 8 && !existingCompetitions.Any(c => c.Tier == 2) && footballAssociation.Balance >= 100000 && footballAssociation.Reputation >= 20)
            {
                var continueLoop = true;
                do
                {
                    Console.Clear();
                    Console.WriteLine("Wil je een [e]xtra club toelaten, of wil je een [l]agere competitie oprichten?");
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.E:
                            continueLoop = false;
                            break;
                        case ConsoleKey.L:
                            Console.WriteLine($"Hoeveel clubs wil je toevoegen aan de lagere competitie van totaal {existingClubs.Count} clubs?");
                            var input = Console.ReadLine();
                            if (int.TryParse(input, out int numberOfClubs) && numberOfClubs > 0 && numberOfClubs < existingClubs.Count)
                            {
                                var ranking = _seasonService.GetRanking(_competitions.First(c => c.CountryId == _userCountryId && c.Tier == 1).Id);
                                var clubsToMove = ranking.TakeLast(numberOfClubs).ToList();
                                var newCompetition = new Competition
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "Division 2",
                                    CountryId = _userCountryId,
                                    Tier = 2,
                                    Type = CompetitionType.League,
                                    Strength = Configuration.MinStrength,
                                };
                                var newRule1 = new CompetitionRules
                                {
                                    CompetitionId = existingCompetitions.First(c => c.Tier == 1).Id,
                                    CompetitionRelegationToId = newCompetition.Id,
                                    RelegationPlaces = 1
                                };
                                var newRule2 = new CompetitionRules
                                {
                                    CompetitionId = newCompetition.Id,
                                    CompetitionPromotionToId = existingCompetitions.First(c => c.Tier == 1).Id,
                                    PromotionPlaces = 1
                                };

                                _competitionService.Add(newCompetition);
                                _competitions = _competitionService.GetCompetitions();
                                _competitionRulesService.Save(newRule1);
                                _competitionRulesService.Save(newRule2);

                                foreach (var club in clubsToMove)
                                {
                                    _clubPerCompetitionService.RemoveClubFromCompetition(club.ClubId, club.CompetitionId);
                                    _clubPerCompetitionService.AddClubToCompetition(club.ClubId, newCompetition.Id);
                                }

                                footballAssociation.Balance -= 100000;

                                return;
                            }
                            else
                            {
                                Console.WriteLine("Ongeldige invoer. Druk op een toets om opnieuw te proberen...");
                                Console.ReadKey(true);
                            }
                            break;
                        default:
                            break;
                    }

                } while (continueLoop);
            }

            if (existingClubs.Count >= 12)
            {
                Console.WriteLine("Er zijn al 12 clubs in de competitie, je kan geen extra club toevoegen.");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                return;
            }

            var newClubs = _clubService.GetEndOfSeasonRequestClubs(_userCountryId, 3, existingClubs.Select(cpc => cpc.ClubId).ToList());
            var counter = 0;

            if (newClubs.Count < 3)
            {
                for (int i = newClubs.Count; i < 3; i++)
                {
                    Console.Write("Kies een club naam om toe te voegen aan de competitie (10 000€): ");
                    var name = Console.ReadLine();
                    var newClub = new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CountryId = _userCountryId,
                        Strength = Configuration.MinStrength
                    };
                    _clubService.Add(newClub);
                    newClubs.Add(newClub);
                    footballAssociation.Balance -= 10000;
                }
            }

            Console.Clear();
            Console.WriteLine("Deze 3 clubs hebben een aanvraag ingediend om toegang te krijgen tot de competitie:");
            foreach (var club in newClubs)
            {
                counter++;
                Console.WriteLine($"{counter}. {club.Name}");
            }
            Console.Write("Geef de nummer van de club die je wilt toevoegen: ");
            var choice = Console.ReadLine();

            var newClubRequested = newClubs[int.Parse(choice) - 1];
            _trainerService.CreateRandomTrainer(newClubRequested.Id);

            var lowestTierCompetition = _competitionService.GetCompetitions().Where(c => c.CountryId == _userCountryId).OrderByDescending(c => c.Tier).First();
            _clubPerCompetitionService.AddClubToCompetition(newClubRequested.Id, lowestTierCompetition.Id);
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

        private void PlayCupGames(DateTime date)
        {
            if (_cupFixtures == null || !_cupFixtures.Any())
                return;
            if (!_cupFixtures.Any(_ => _.MatchDay == date))
                return;

            var userCountry = _clubService.GetClubById(_userCountryId).CountryId;
            var cupCompetitions = _competitions
                .Where(_ => _.Type == Competition.CompetitionType.Cup)
                .ToList();

            Console.Clear();
            Console.WriteLine("=== National Cup Fixtures ===");
            Console.WriteLine();

            foreach (var cupCompetition in cupCompetitions)
            {
                var display = cupCompetition.CountryId == userCountry;
                if (display)
                    Console.WriteLine($"--- {cupCompetition.Name} ---");
                var fixturesForCompetition = _cupFixtures
                    .Where(_ => _.CompetitionId == cupCompetition.Id && _.MatchDay == date)
                    .ToList();

                if (fixturesForCompetition.Count == 0)
                    continue;

                if (display)
                    Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} ===");

                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId == Guid.Empty || fixture.AwayTeamId == Guid.Empty)
                        continue;

                    if (fixture.HomeTeamId == _userCountryId || fixture.AwayTeamId == _userCountryId)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    var homeTier = GetClubTier(fixture.HomeTeamId);
                    var awayTier = GetClubTier(fixture.AwayTeamId);

                    if (display)
                        Console.WriteLine(
                        $"{fixture.HomeTeam.Name} ({homeTier}) vs {fixture.AwayTeam.Name} ({awayTier})"
                    );

                    Console.ResetColor();
                }

                if (display)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to play this round...");
                    Console.ReadKey();
                }
                // Speel enkel deze ronde
                _seasonService.PlayMatchDay(fixturesForCompetition, date, true, _userCountryId, true);
                if (display)
                {
                    Console.Clear();
                    Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} Results ===");
                }
                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId != Guid.Empty && fixture.AwayTeamId != Guid.Empty)
                    {

                        if (fixture.HomeTeamId == _userCountryId || fixture.AwayTeamId == _userCountryId)
                            Console.ForegroundColor = ConsoleColor.Yellow;

                        var homeTier = GetClubTier(fixture.HomeTeamId);
                        var awayTier = GetClubTier(fixture.AwayTeamId);

                        if (display)
                            Console.WriteLine(
                            $"{fixture.HomeTeam.Name} ({homeTier}) {fixture.HomeScore} - {fixture.AwayScore} {fixture.AwayTeam.Name} ({awayTier})"
                        );
                        Console.ResetColor();
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

                if (display)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        private void PlayInternationalGames(DateTime date)
        {
            if (_internationalFixtures == null || !_internationalFixtures.Any())
                return;
            if (!_internationalFixtures.Any(_ => _.MatchDay == date))
                return;

            var fixturesForRound = _internationalFixtures
                .Where(_ => _.MatchDay == date)
                .ToList();

            if (fixturesForRound.Count == 0)
                return;

            Console.Clear();
            Console.WriteLine($"=== International Round {date} ===");
            Console.WriteLine();

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

                Console.WriteLine($"{homeName} vs {awayName}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to play this round...");
            Console.ReadKey();

            _seasonService.PlayMatchDay(fixturesForRound, date, true, _userCountryId, true);

            Console.Clear();
            Console.WriteLine($"=== International Round {date} Results ===");
            foreach (var fixture in fixturesForRound)
            {
                if (fixture.HomeTeamId != Guid.Empty && fixture.AwayTeamId != Guid.Empty)
                    Console.WriteLine($"{fixture.HomeTeam.Name} {fixture.HomeScore} - {fixture.AwayScore} {fixture.AwayTeam.Name}");

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

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            // Winnaar bepalen – finale
            var final = _internationalFixtures
                .OrderByDescending(f => f.RoundNo)
                .First();

            if (final.MatchDay != date)
                return;

            var finalWinner = final.HomeScore > final.AwayScore ? final.HomeTeam : final.AwayTeam;
            Console.WriteLine($"International Cup winner: {finalWinner.Name}!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private IList<ClubLeagueCompetition> DisplayLeagueTable(Guid competitionId, Guid? highlightClubId = null)
        {
            const int positionWidth = 4;
            const int nameWidth = 25;
            const int gamesWidth = 8;
            const int wonWidth = 8;
            const int drawWidth = 8;
            const int lostWidth = 8;
            const int gfWidth = 6;
            const int gaWidth = 6;
            const int gdWidth = 6;
            const int pointsWidth = 8;

            var competitionToShow = _competitions.First(_ => _.Id == competitionId);

            Console.Clear();
            Console.WriteLine($"=== League Table: {competitionToShow.Name} ===");
            Console.WriteLine();

            Console.WriteLine(
                $"{"P".PadRight(positionWidth)}" +
                $"{"Club".PadRight(nameWidth)}" +
                $"{"Games".PadLeft(gamesWidth)}" +
                $"{"Won".PadLeft(wonWidth)}" +
                $"{"Draw".PadLeft(drawWidth)}" +
                $"{"Lost".PadLeft(lostWidth)}" +
                $"{"GF".PadLeft(gfWidth)}" +
                $"{"GA".PadLeft(gaWidth)}" +
                $"{"GD".PadLeft(gdWidth)}" +
                $"{"Points".PadLeft(pointsWidth)}"
            );

            Console.WriteLine(new string('-', positionWidth + nameWidth + gamesWidth + wonWidth + drawWidth + lostWidth + pointsWidth + gfWidth + gaWidth + gdWidth));

            var counter = 1;
            var table = _seasonService.ClubLeagueCompetitions
                .Where(_ => _.CompetitionId == competitionToShow.Id)
                .OrderByDescending(_ => _.Points)
                .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
                .ThenByDescending(_ => _.GoalsFor)
                .ToList();

            foreach (var c in table)
            {
                var club = _clubService.GetClubById(c.ClubId);

                // Alleen highlighten als er een club meegegeven is
                if (highlightClubId.HasValue && c.ClubId == highlightClubId.Value)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(
                    $"{counter.ToString().PadRight(positionWidth)}" +
                    $"{club.Name.PadLeft(nameWidth)}" +
                    $"{c.MatchesPlayed.ToString().PadLeft(gamesWidth)}" +
                    $"{c.Won.ToString().PadLeft(wonWidth)}" +
                    $"{c.Draw.ToString().PadLeft(drawWidth)}" +
                    $"{c.Lost.ToString().PadLeft(lostWidth)}" +
                    $"{c.GoalsFor.ToString().PadLeft(gfWidth)}" +
                    $"{c.GoalsAgainst.ToString().PadLeft(gaWidth)}" +
                    $"{c.GoalDifference.ToString().PadLeft(gdWidth)}" +
                    $"{c.Points.ToString().PadLeft(pointsWidth)}"
                );

                Console.ResetColor();
                counter++;
            }

            return table;
        }

        private void DisplayResult(Competition competitionToShow, DateTime date)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Date {date} - {competitionToShow.Name} ===");
            Console.WriteLine();

            var fixturesForMatchDay = _fixtures
                .Where(_ => _.MatchDay == date && _.CompetitionId == competitionToShow.Id)
                .ToList();

            if (fixturesForMatchDay.Count == 0)
                return;

            int homeWidth = fixturesForMatchDay.Max(f => f.HomeTeam.Name.Length) + 2;
            int awayWidth = fixturesForMatchDay.Max(f => f.AwayTeam.Name.Length) + 2;

            Console.WriteLine(
                $"{"Home Team".PadRight(homeWidth)}" +
                $"{"Score".PadRight(8)}" +
                $"{"Away Team".PadRight(awayWidth)}"
            );

            Console.WriteLine(new string('-', homeWidth + 8 + awayWidth));

            foreach (var fixture in fixturesForMatchDay)
            {
                var score = $"{fixture.HomeScore} - {fixture.AwayScore}";
                if (fixture.HomeTeamId == _userCountryId || fixture.AwayTeamId == _userCountryId)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(
                    $"{fixture.HomeTeam.Name.PadRight(homeWidth)}" +
                    $"{score.PadRight(8)}" +
                    $"{fixture.AwayTeam.Name.PadRight(awayWidth)}"
                );
                Console.ResetColor();
            }

            Console.WriteLine();
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

        private bool DisplayNextFixture(Competition competitionToShow, DateTime fromDate)
        {
            var nextDate = _fixtures
                .Where(fixture =>
                    fixture.CompetitionId == competitionToShow.Id &&
                    fixture.MatchDay >= fromDate &&
                    (fixture.AwayTeam.CountryId == _userCountryId || fixture.HomeTeam.CountryId == _userCountryId))
                .Select(fixture => fixture.MatchDay)
                .OrderBy(date => date)
                .FirstOrDefault();

            if (nextDate == default || nextDate >= _newSeasonDate)
            {
                Console.WriteLine("Geen volgende competitiewedstrijd gevonden.");
                return false;
            }

            var fixtures = _fixtures.Where(f =>
                f.CompetitionId == competitionToShow.Id &&
                f.MatchDay == nextDate &&
                (f.AwayTeam.CountryId == _userCountryId || f.HomeTeam.CountryId == _userCountryId));

            Console.WriteLine($"Volgende wedstrijd: {nextDate:dddd dd/MM/yyyy}");
            Console.WriteLine(new string('-', 40));
            foreach (var f in fixtures)
            {
                Console.WriteLine($"{f.HomeTeam.Name} vs {f.AwayTeam.Name}");
            }

            return true;
        }

        private bool UserPlaysOn(DateTime date)
        {
            return _fixtures.Any(fixture =>
            fixture.MatchDay == date &&
                       (fixture.AwayTeam.CountryId == _userCountryId || fixture.HomeTeam.CountryId == _userCountryId)) ||
                   _cupFixtures.Any(fixture =>
                       fixture.MatchDay == date &&
                       (fixture.AwayTeam.CountryId == _userCountryId || fixture.HomeTeam.CountryId == _userCountryId)) ||
                   (_internationalFixtures?.Any(fixture =>
                       fixture.MatchDay == date &&
                       (fixture.HomeTeam.CountryId == _userCountryId || fixture.AwayTeam.CountryId == _userCountryId)) ?? false);
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

        private bool ResetClubStrength(IList<Club> clubs, Country country)
        {
            var clubsInCountry = clubs
                .Where(c => c.CountryId == country.Id)
                .ToList();

            if (!clubsInCountry.Any())
                return false;

            var range = Configuration.MaxStrength - Configuration.MinStrength;
            var counter = clubsInCountry.Count / (range == 0 ? 1 : range);
            var currentStrength = Configuration.MaxStrength;

            for (int i = 0; i < clubsInCountry.Count; i++)
            {
                var club = clubsInCountry[i];
                club.Strength = club.Strength > 0 ? currentStrength : 1;
                _clubService.Update(club);

                counter--;
                if (counter <= 0)
                {
                    currentStrength--;
                    counter = clubsInCountry.Count / (range == 0 ? 1 : range);
                }
            }

            return true;
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
                        // Geen highlight: gewoon de tabel tonen
                        DisplayLeagueTable(competition.Id);
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
