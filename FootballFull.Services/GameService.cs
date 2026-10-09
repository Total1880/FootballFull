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
        private readonly IMatchdayService _matchdayService;
        private readonly IGameInitializationService _gameInitializationService;
        private readonly IFixtureService _fixtureService;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly ISeasonEconomyService _seasonEconomyService;
        private readonly IEndOfSeasonChoicesService _endOfSeasonChoicesService;
        private readonly IStrengthService _strengthService;
        private readonly IEndOfSeasonService _endOfSeasonService;
        private readonly IGameUI _gameUI;

        private IList<ClubPerCompetition> _clubsPerCompetition = new List<ClubPerCompetition>();
        private IList<Competition> _competitions = new List<Competition>();
        private IList<Fixture> _fixtures = new List<Fixture>();
        private IList<Fixture> _cupFixtures = new List<Fixture>();
        private IList<Fixture>? _internationalFixtures;
        private IList<FootballAssociation> _footballAssociations = new List<FootballAssociation>();

        private Guid _userCountryId;
        private DateTime _currentDate;
        private DateTime _newSeasonDate;
        private int _year;

        public GameService(
            ISeasonService seasonService,
            IFixtureService fixtureService,
            IMatchdayService matchdayService,
            IGameInitializationService gameInitializationService,
            IClubService clubService,
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            IEndOfSeasonChoicesService endOfSeasonChoicesService,
            ISeasonEconomyService seasonEconomyService,
            IStrengthService strengthService,
            IEndOfSeasonService endOfSeasonService,
            IGameUI gameUI)
        {
            _seasonService = seasonService;
            _fixtureService = fixtureService;
            _matchdayService = matchdayService;
            _gameInitializationService = gameInitializationService;
            _clubService = clubService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _endOfSeasonChoicesService = endOfSeasonChoicesService;
            _seasonEconomyService = seasonEconomyService;
            _strengthService = strengthService;
            _endOfSeasonService = endOfSeasonService;
            _gameUI = gameUI;

        }

        public void Run(bool isNew)
        {
            var initial = _gameInitializationService.Initialize(isNew);

            _userCountryId = initial.UserCountryId;
            _year = initial.Year;
            _currentDate = initial.CurrentDate;
            _newSeasonDate = initial.NewSeasonDate;
            _clubsPerCompetition = initial.ClubsPerCompetition;
            _competitions = initial.Competitions;
            _fixtures = initial.LeagueFixtures;
            _cupFixtures = initial.CupFixtures;
            _internationalFixtures = initial.InternationalFixtures;
            _footballAssociations = initial.FootballAssociations;

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
                        ContinueGame();
                        break;
                    case MainMenuChoice.ShowOtherCompetitions:
                        var selectedCompetitionId = _gameUI.ChooseCompetitions(GameUIMapper.Options(
                            _competitions.Where(c => c.Type == CompetitionType.League)));
                        if (selectedCompetitionId.HasValue)
                            _gameUI.ShowTable(CreateTableDashBoard(selectedCompetitionId.Value), true);
                        break;
                    case MainMenuChoice.ShowInternationalRankings:
                        _gameUI.ShowInternationRankings(GameUIMapper.Rankings(DisplayInternationalRankingPerYear()), _year, true);
                        break;
                    case MainMenuChoice.Save:
                        _seasonService.SaveGame();
                        _gameUI.ShowMessage("Spel opgeslagen", "Je spel werd succesvol opgeslagen.", true);
                        break;

                    case MainMenuChoice.SaveAndExit:
                        _seasonService.SaveGame();
                        return;
                }
            }
        }

        private void ContinueGame()
        {
            var gamesToShow = PlayUntilNextMatchday();
            var league = _competitions.First(c =>
                c.CountryId == _userCountryId &&
                c.Type == CompetitionType.League &&
                c.Tier == 1);

            _gameUI.ShowTable(CreateTableDashBoard(league.Id));

            if (gamesToShow?.LeagueCompetition == true)
                _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(league.Id, _currentDate)));

            _gameUI.ShowFixtures(
                GameUIMapper.Fixtures(GetNextFixture(league.Id, _currentDate)), true);

            if (gamesToShow?.CupCompetition == true)
            {
                var cup = _competitions.FirstOrDefault(c =>
                    c.CountryId == _userCountryId && c.Type == CompetitionType.Cup);
                if (cup != null)
                    _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(cup.Id, _currentDate)), true);
            }

            if (gamesToShow?.InternationalCompetition == true)
            {
                var international = _competitions.FirstOrDefault(c =>
                    c.Type == CompetitionType.International);
                if (international != null)
                    _gameUI.ShowResults(
                        GameUIMapper.Fixtures(GetResult(international.Id, _currentDate)), true);
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
                    MatchesPlayed = rank.MatchesPlayed,
                    Won = rank.Won,
                    Draw = rank.Draw,
                    Lost = rank.Lost,
                    GoalsFor = rank.GoalsFor,
                    GoalsAgainst = rank.GoalsAgainst,
                    GoalDifference = rank.GoalDifference,
                    Points = rank.Points
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
                PlayRemainingSeasonGames();

                FinishSeason();
                return null;
            }

            while (_currentDate <= nextMatchday.Value)
            {


                PlayGamesForCurrentDate(weekGamesToShow);
                _currentDate = _currentDate.AddDays(1);
            }

            return weekGamesToShow;
        }

        private void PlayGamesForCurrentDate(WeekGamesToShow? weekGamesToShow)
        {
            _matchdayService.PlayDate(new MatchdayRequest(
                _currentDate,
                _userCountryId,
                _competitions,
                _fixtures,
                _cupFixtures,
                _internationalFixtures,
                weekGamesToShow));
        }

        private void PlayRemainingSeasonGames()
        {
            while (_currentDate < _newSeasonDate)
            {
                // The remaining fixtures must be played for every country.
                PlayGamesForCurrentDate(null);
                _currentDate = _currentDate.AddDays(1);
            }
        }

        private IEnumerable<Fixture> GetAllFixtures()
        {
            return _fixtures
                .Concat(_cupFixtures)
                .Concat(_internationalFixtures ?? Array.Empty<Fixture>());
        }

        private DateTime? GetNextMatchday()
        {
            var competitionIds = _competitions
                .Where(c => c.CountryId == _userCountryId || c.Type == CompetitionType.International)
                .Select(c => c.Id)
                .ToHashSet();

            return GetAllFixtures()
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

            return GetAllFixtures()
                .Count(f => f.MatchDay == date &&
                    (competitionIds.Contains(f.CompetitionId) ||
                     f.HomeTeam?.CountryId == _userCountryId ||
                     f.AwayTeam?.CountryId == _userCountryId));
        }

        private void FinishSeason()
        {
            ShowSeasonClosing();
            ProcessSeasonResults();
            ApplyEndOfSeasonDecisions();
            StartNextSeason();
            _seasonService.SaveGame();
        }

        private void ShowSeasonClosing()
        {
            _gameUI.ShowMessage("Seizoen afgelopen", $"Seizoen {_year}/{_year + 1} is afgelopen.");

            // Keep the final standings available before starting a new season.
            if (_gameUI.AskYesNoQuestion(
                "Wil je de eindstanden van de competities bekijken?",
                defaultAnswer: true))
            {
                ShowEndOfSeasonTables();
            }
        }

        private void ProcessSeasonResults()
        {
            // Preserve the current ordering: international fixtures are initialized
            // before season finances and club development are processed.
            _endOfSeasonService.ProcessSeasonResults();
            _internationalFixtures = _seasonService.InitializeInternationalGames(_newSeasonDate);
            _gameUI.ShowSeasonFinancialResult(
                GameUIMapper.Finances(_seasonEconomyService.CalculateAssociationResult(
                    _userCountryId, _year, _competitions, GetUserAssociation())), true);

            _seasonEconomyService.ProcessClubFinancialResults();
            _strengthService.RecalculateClubStrengths();
        }

        private void ApplyEndOfSeasonDecisions()
        {
            var developedClubs = _clubService.GetClubs()
                .Where(c => c.CountryId == _userCountryId &&
                            c.LastSeasonFinancialResult != null);

            if (_endOfSeasonChoicesService.HandleChoices(_userCountryId, GetUserAssociation()))
                _competitions = _competitionService.GetCompetitions();
            _gameUI.ShowClubDevelopment(
                GameUIMapper.ClubDevelopment(developedClubs), true);
            _gameUI.ShowSeasonEvent(GameUIMapper.Event(_seasonEconomyService.ApplyRandomEvent(GetUserAssociation())), true);
        }

        private void StartNextSeason()
        {
            _year++;
            _seasonService.Year = _year;

            var association = _footballAssociations
                .First(fa => fa.CountryId == _userCountryId);
            _seasonEconomyService.AllocateClubSubsidies(_userCountryId, _year, association);

            _currentDate = new DateTime(_year, 7, 1);
            _newSeasonDate = _currentDate.AddYears(1);

            _clubsPerCompetition = _seasonService.InitializeNewSeason(_year);
            _strengthService.RecalculateCompetitionStrengths(_year);
            _competitions = _competitionService.GetCompetitions();
            _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
            _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);
        }

        private FootballAssociation GetUserAssociation()
        {
            return _footballAssociations.First(fa => fa.CountryId == _userCountryId);
        }

        private void ShowNews(DateTime date, Guid competitionId)
        {
            // Eén query, maar we vermijden dubbele enumeratie door te materializen als nodig
            var matches = _seasonService.NewsMessages.Where(nm =>
                nm.CountryId == _userCountryId &&
                nm.CompetitionId == competitionId &&
                nm.Date >= date.AddDays(-7) &&
                nm.Date <= date);

            _gameUI.ShowNews(GameUIMapper.News(matches));
        }

        private List<Fixture> GetResult(Guid competitionId, DateTime date)
        {
            var lastDate = GetAllFixtures()
    .Where(fixture =>
        fixture.CompetitionId == competitionId &&
        fixture.MatchDay <= date)
    .Select(fixture => fixture.MatchDay)
    .OrderByDescending(date => date)
    .FirstOrDefault();

            return GetAllFixtures()
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

        private void ShowEndOfSeasonTables()
        {
            var leagueCompetitions = _competitions
                .Where(c => c.Type == CompetitionType.League)
                .OrderBy(c => c.CountryId == _userCountryId ? 0 : 1)
                .ThenBy(c => c.CountryId)
                .ThenBy(c => c.Tier)
                .ToList();

            while (true)
            {
                var selectedCompetitionId = _gameUI.ChooseCompetitions(
                    GameUIMapper.Options(leagueCompetitions));

                if (!selectedCompetitionId.HasValue)
                    return;

                _gameUI.ShowTable(
                    CreateTableDashBoard(selectedCompetitionId.Value),
                    true);
            }
        }
    }
}
