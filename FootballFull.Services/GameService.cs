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
                ShowNews(_currentDate, _competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id);
                var dashboard = CreateDashboard();

                var choice = _gameUI.ShowMainMenu(dashboard);

                switch (choice)
                {
                    case MainMenuChoice.Continue:
                        var gamesToShow = PlayUntilNextMatchday();

                        _gameUI.ShowTable(CreateTableDashBoard(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id));
                        if (gamesToShow != null && gamesToShow.LeagueCompetition)
                            _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id, _currentDate)));
                        _gameUI.ShowFixtures(GameUIMapper.Fixtures(GetNextFixture(_competitions.First(_ => _.CountryId == _userCountryId && _.Tier == 1).Id, _currentDate)), true);

                        if (gamesToShow != null && gamesToShow.CupCompetition)
                            _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(_competitions.First(_ => _.CountryId == _userCountryId && _.Type == CompetitionType.Cup).Id, _currentDate)), true);
                        if (gamesToShow != null && gamesToShow.InternationalCompetition)
                            _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(_competitions.First(_ => _.Type == CompetitionType.International).Id, _currentDate)), true);
                        break;
                    case MainMenuChoice.ShowOtherCompetitions:
                        var selectedCompetitionId = _gameUI.ChooseCompetitions(GameUIMapper.Options(
                            _competitions.Where(c => c.Type == CompetitionType.League &&
                                !(c.CountryId == _userCountryId && c.Tier == 1))));
                        if (selectedCompetitionId.HasValue)
                            _gameUI.ShowTable(CreateTableDashBoard(selectedCompetitionId.Value), true);
                        break;
                    case MainMenuChoice.ShowInternationalRankings:
                        _gameUI.ShowInternationRankings(GameUIMapper.Rankings(DisplayInternationalRankingPerYear()), _year, true);
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
                Rows = ranking.Select(rank => new CompetitionTableRowViewModel
                {
                    ClubName = rank.Club?.Name ?? rank.ClubId.ToString(),
                    MatchesPlayed = rank.MatchesPlayed, Won = rank.Won, Draw = rank.Draw, Lost = rank.Lost,
                    GoalsFor = rank.GoalsFor, GoalsAgainst = rank.GoalsAgainst,
                    GoalDifference = rank.GoalDifference, Points = rank.Points
                }).ToList()
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
            _gameUI.ShowSeasonFinancialResult(GameUIMapper.Finances(CalculateSeasonFinancialResult()), true);
            CalculateClubFinancialResults();
            _strengthService.RecalculateClubStrengths();
            _strengthService.RecalculateCompetitionStrengths(_seasonService.Year);
            _gameUI.ShowSeasonEvent(GameUIMapper.Event(Events()), true);
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

        private SeasonEvent Events()
        {
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);

            var events = _seasonEventService.GetRandomSeasonEvents();


            footballAssociation.Balance += events.BalanceChange;
            footballAssociation.Reputation += events.ReputationChange;

            _footballAssociationsService.Update(footballAssociation);

            return events;
        }

        private SeasonFinancialResult CalculateSeasonFinancialResult()
        {
            var existingClubs = _clubPerCompetitionService.GetAllClubPerCompetitionForCountry(_userCountryId);
            var footballAssociation = _footballAssociations.First(fa => fa.CountryId == _userCountryId);

            var seasonFinancialResult = _seasonFinancialResultService.CalculateFinancialResults(existingClubs.Count, _competitions.Where(c => c.CountryId == _userCountryId).Count(), footballAssociation);

            _footballAssociationsService.Update(footballAssociation);

            return seasonFinancialResult;
        }

        private void CalculateClubFinancialResults()
        {
            foreach (var country in _countryService.GetCountries())
            {
                _endOfSeasonService.ProcessClubFinances(country.Id);
            }
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
                _gameUI.ShowMessage("ERROR", options.CannotAddClubReason);
                return;
            }

            if (options.CanCreateLowerDivision &&
                _gameUI.AskToCreateLowerDivision())
            {
                var numberOfClubs = _gameUI.AskForClubsToMove(
                    maximumClubsToMove: options.CurrentClubCount - 2,
                    currentClubCount: options.CurrentClubCount,
                    minimumClubsToMove: 2);

                _endOfSeasonService.CreateLowerDivision(
                    _userCountryId,
                    numberOfClubs,
                    footballAssociation);

                _competitions = _competitionService.GetCompetitions();

                return;
            }

            if (!options.CanAddClub)
            {
                _gameUI.ShowMessage("ERROR", options.CannotAddClubReason);
                return;
            }

            if (!_gameUI.AskYesNoQuestion($"Wil je een nieuwe club toevoegen? Kostprijs {Configuration.NewClubCost} (y/n)", defaultAnswer: false))
                return;

            var applicants = _endOfSeasonService.GetApplicantClubs(
                _userCountryId,
                footballAssociation,
                3);

            while (applicants.Count < 3)
            {
                _gameUI.ShowMessage("Er zijn nog kandidaat-clubs nodig.", $"Er zijn nog {3 - applicants.Count} kandidaat-club(s) nodig.");

                var clubName = _gameUI.AskForInput("Geef de naam van de nieuwe club: ", "");

                if (string.IsNullOrWhiteSpace(clubName))
                {
                    _gameUI.ShowMessage("ERROR","De naam van een club mag niet leeg zijn.");
                    continue;
                }

                var newClub = _endOfSeasonService.CreateApplicantClub(
                    _userCountryId,
                    clubName);

                applicants.Add(newClub);
            }

            var selectedClub = _gameUI.AskPlayerToSelectClub(GameUIMapper.Options(applicants));

            _endOfSeasonService.AdmitClub(
                _userCountryId,
                selectedClub,
                footballAssociation);
        }



        private void ShowNews(DateTime date, Guid competitionId)
        {

            // Eén query, maar we vermijden dubbele enumeratie door te materializen als nodig
            var matches = _seasonService.NewsMessages.Where(nm =>
                nm.CountryId == _userCountryId &&
                nm.CompetitionId == competitionId &&
                nm.Date == date);

            _gameUI.ShowNews(GameUIMapper.News(matches));


        }


        #region Helpers

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
                .Concat(_internationalFixtures ?? Array.Empty<Fixture>())
    .Where(fixture =>
        fixture.CompetitionId == competitionId &&
        fixture.MatchDay <= date)
    .Select(fixture => fixture.MatchDay)
    .OrderByDescending(date => date)
    .FirstOrDefault();

            return _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures ?? Array.Empty<Fixture>())
                .Where(_ => _.MatchDay == lastDate && _.CompetitionId == competitionId)
                .ToList();
        }

        private List<CountryCoefficientRanking> DisplayInternationalRankingPerYear()
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

            return rankings;
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

            while (true)
            {
                var chosenIndex = _gameUI.AskPlayerToSelectCountry(GameUIMapper.Options(countries));

                // Bestaand land
                if (chosenIndex > 0 && chosenIndex <= countries.Count)
                {
                    var chosenCountry = countries[chosenIndex - 1];

                    _gameUI.ShowMessage("Keuze", $"Je hebt gekozen: {chosenCountry.Name}");

                    return chosenCountry.Id;
                }

                // Nieuw land
                if (chosenIndex == 0)
                {
                    var newCountryName = _gameUI.AskNewCountryName();

                    var newCountry = new Country
                    {
                        Id = Guid.NewGuid(),
                        Name = newCountryName
                    };

                    _countryService.Add(newCountry);

                    _userCountryId = newCountry.Id;

                    _gameUI.ShowMessage(
                        "Nieuw land aangemaakt",
                        $"Nieuw land '{newCountry.Name}' werd aangemaakt.");

                    CreateStarterClubs();

                    return newCountry.Id;
                }

                _gameUI.ShowMessage("Ongeldige keuze", "Ongeldige keuze. Probeer opnieuw.");
            }
        }

        private void CreateStarterClubs()
        {
            var starterClubNames = _gameUI.AskStarterClubNames(6);
            foreach(var name in starterClubNames)
                _clubService.Add(new Club { Name = name, CountryId = _userCountryId, Strength = new Random().Next(OlavFramework.Configuration.MinStrength, 4) });
        }
        #endregion
    }
}
