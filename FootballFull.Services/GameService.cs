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
        private readonly IHistoryService _historyService;
        private readonly IInternationalRankingService _internationalRankingService;
        private readonly IPlayerCompetitionSettingsService _playerCompetitionSettingsService;
        private readonly ISaveDataService _saveDataService;
        private readonly IGameUI _gameUI;

        private IList<ClubPerCompetition> _clubsPerCompetition = new List<ClubPerCompetition>();
        private IList<Fixture> _fixtures = new List<Fixture>();
        private IList<Fixture> _cupFixtures = new List<Fixture>();
        private IList<Fixture>? _internationalFixtures;
        private IList<FootballAssociation> _footballAssociations = new List<FootballAssociation>();

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
            IHistoryService historyService,
            IInternationalQualificationPlacesService internationalQualificationPlacesService,
            IInternationalRankingService internationalRankingService,
            IPlayerCompetitionSettingsService playerCompetitionSettingsService,
            ISaveDataService saveDataService,
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
            _historyService = historyService;
            _internationalRankingService = internationalRankingService;
            _playerCompetitionSettingsService = playerCompetitionSettingsService;
            _saveDataService = saveDataService;
            _gameUI = gameUI;
        }

        public void Run(bool isNew)
        {
            var initial = _gameInitializationService.Initialize(isNew);

            _saveDataService.SaveData.UserCountryId = initial.UserCountryId;
            _year = initial.Year;
            _currentDate = initial.CurrentDate;
            _newSeasonDate = initial.NewSeasonDate;
            _clubsPerCompetition = initial.ClubsPerCompetition;
            _fixtures = initial.LeagueFixtures;
            _cupFixtures = initial.CupFixtures;
            _internationalFixtures = initial.InternationalFixtures;
            _footballAssociations = initial.FootballAssociations;

            ConfigureSeasonPrizeMoney();
            GameLoop();
        }

        private void GameLoop()
        {
            while (true)
            {
                ShowNews(_currentDate, _competitionService.GetCompetitions().First(_ => _.CountryId == _saveDataService.SaveData.UserCountryId && _.Tier == 1).Id);
                var dashboard = CreateDashboard();

                var choice = _gameUI.ShowMainMenu(dashboard);

                switch (choice)
                {
                    case MainMenuChoice.Continue:
                        ContinueGame();
                        break;
                    case MainMenuChoice.ShowOtherCompetitions:
                        var selectedCompetitionId = _gameUI.ChooseCompetitions(GameUIMapper.Options(
                            _competitionService.GetCompetitions().Where(c => c.Type == CompetitionType.League)));
                        if (selectedCompetitionId.HasValue)
                            _gameUI.ShowTable(CreateTableDashBoard(selectedCompetitionId.Value), true);
                        break;
                    case MainMenuChoice.ShowInternationalRankings:
                        _gameUI.ShowInternationRankings(GameUIMapper.Rankings(new List<CountryInternationalCompetitionRanking> { DisplayInternationalRankingPerYear() }), _year, true, true);
                        break;
                    case MainMenuChoice.ShowCompetitionHistory:
                        selectedCompetitionId = _gameUI.ChooseCompetitions(GameUIMapper.Options(
    _competitionService.GetCompetitions().Where(c => c.Type != CompetitionType.ParentCompetition)));
                        if (selectedCompetitionId.HasValue)
                            _gameUI.ShowCompetitionHistory(GameUIMapper.CompetitionHistory(_historyService.GetHistoryForCompetitionId(selectedCompetitionId.Value)));
                        break;
                    case MainMenuChoice.ShowClubHistory:
                        var selectClubId = _gameUI.ChooseClub(GameUIMapper.Options(_clubService.GetClubs()));
                        if (selectClubId.HasValue)
                            _gameUI.ShowClubHistory(GameUIMapper.ClubHistory(_historyService.GetHistoryForClubId(selectClubId.Value)));
                        break;
                    case MainMenuChoice.Settings:
                        UpdateSettings();
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

        private void UpdateSettings()
        {
            var settings = _playerCompetitionSettingsService.GetSettings();
            var viewModel = GameUIMapper.PlayerCompetitionSettings(settings, _competitionService.GetCompetitionsForCountry(_saveDataService.SaveData.UserCountryId));
            var choice = _gameUI.ShowSettings(viewModel);

            switch (choice)
            {
                case SettingsChoice.AutoAddClub:
                    settings.AutoAddClub = _gameUI.AskYesNoQuestion("Wil je automatisch clubs laten toevoegen? (y/n)");
                    break;
                case SettingsChoice.AutoAddCompetition:
                    settings.AutoAddCompetition = _gameUI.AskYesNoQuestion("Wil je automatisch competities laten toevoegen? (y/n)");
                    break;
                case SettingsChoice.AutoAddCompetitionThreshold:
                    int result;
                    if (int.TryParse(_gameUI.AskForInput("Wat is het minimum aantal clubs dat in de laagste divisie moet zitten vooraleer een nieuwe competitie wordt opgericht?"), out result))
                        settings.AutoAddCompetitionThreshold = result;
                    break;
                case SettingsChoice.AutoAddFeederClub:
                    settings.AutoAddFeederClub = _gameUI.AskYesNoQuestion("Wil je automatisch feeder clubs laten toevoegen? (y/n)");
                    break;
                case SettingsChoice.PrizeMoneyPerCompetition:
                    SettingsPrizeMoney(settings);
                    break;
                case SettingsChoice.Back:
                    break;
            }

            _playerCompetitionSettingsService.UpdateSetting(settings);

        }

        private void SettingsPrizeMoney(PlayerCompetitionSettings settings)
        {
            // Show settings
            // Ask to update auto prize settings, or 1 of the competitions

            settings.AutoPrizeMoney = _gameUI.AskYesNoQuestion("Wil je elk seizoen automatisch het prijzengeld geven?");

            if (settings.AutoPrizeMoney)
            {
                var competition = _gameUI.ChooseCompetitions(GameUIMapper.Options(_competitionService.GetCompetitions().Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId)));
                var prizeMoney = _gameUI.AskPrizeMoneyBudget(_competitionService.GetCompetitionById((Guid)competition).Name);
                var distribution = _gameUI.AskPrizeMoneyDistribution(_competitionService.GetCompetitionById((Guid)competition).Type);

                settings.PrizeMoneyPerCompetition[(Guid)competition] = prizeMoney;
                settings.PrizeMoneyDistributionPerCompetition[(Guid)competition] = distribution;
            }
        }

        private void ContinueGame()
        {
            var gamesToShow = PlayUntilNextMatchday();
            var league = _competitionService.GetCompetitions().First(c =>
                c.CountryId == _saveDataService.SaveData.UserCountryId &&
                c.Type == CompetitionType.League &&
                c.Tier == 1);

            _gameUI.ShowTable(CreateTableDashBoard(league.Id));

            if (gamesToShow?.LeagueCompetition == true)
                _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(league.Id, _currentDate)));

            _gameUI.ShowFixtures(
                GameUIMapper.Fixtures(GetNextFixture(league.Id, _currentDate)), true);

            if (gamesToShow?.CupCompetition == true)
            {
                var cup = _competitionService.GetCompetitions().FirstOrDefault(c =>
                    c.CountryId == _saveDataService.SaveData.UserCountryId && c.Type == CompetitionType.Cup);
                if (cup != null)
                    _gameUI.ShowResults(GameUIMapper.Fixtures(GetResult(cup.Id, _currentDate)), true);
            }

            if (gamesToShow?.InternationalCompetition == true)
            {
                var international = _competitionService.GetCompetitions().FirstOrDefault(c =>
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
                CompetitionName = _competitionService.GetCompetitions().First(_ => _.Id == competitionId).Name,
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
            var association = _footballAssociations.Single(fa => fa.CountryId == _saveDataService.SaveData.UserCountryId);
            var competitionIds = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId && c.Type == CompetitionType.League)
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
                    .GetAllClubPerCompetitionForCountry(_saveDataService.SaveData.UserCountryId).Count,
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
                _saveDataService.SaveData.UserCountryId,
                _competitionService.GetCompetitions(),
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
            var competitionIds = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId || c.Type == CompetitionType.International)
                .Select(c => c.Id)
                .ToHashSet();

            return GetAllFixtures()
                .Where(f => f.MatchDay >= _currentDate)
                .Where(f => competitionIds.Contains(f.CompetitionId) ||
                            f.HomeTeam?.CountryId == _saveDataService.SaveData.UserCountryId ||
                            f.AwayTeam?.CountryId == _saveDataService.SaveData.UserCountryId)
                .Select(f => f.MatchDay)
                .OrderBy(date => date)
                .Cast<DateTime?>()
                .FirstOrDefault();
        }

        private int CountMatchesOn(DateTime date)
        {
            var competitionIds = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId)
                .Select(c => c.Id)
                .ToHashSet();

            return GetAllFixtures()
                .Count(f => f.MatchDay == date &&
                    (competitionIds.Contains(f.CompetitionId) ||
                     f.HomeTeam?.CountryId == _saveDataService.SaveData.UserCountryId ||
                     f.AwayTeam?.CountryId == _saveDataService.SaveData.UserCountryId));
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
            _endOfSeasonService.ProcessSeasonResults(_cupFixtures, _internationalFixtures);
            _endOfSeasonService.ProcessPrizeMoney(_cupFixtures, _internationalFixtures);
            _endOfSeasonService.ProcessFeederClubAnalyse();
            _gameUI.ShowSeasonFinancialResult(GameUIMapper.Finances(_seasonEconomyService.CalculateAssociationResult(
                    _saveDataService.SaveData.UserCountryId, _year, _competitionService.GetCompetitions(), GetUserAssociation())), true);

            _seasonEconomyService.ProcessClubFinancialResults();
            _strengthService.RecalculateClubStrengths();
            _internationalFixtures = _seasonService.InitializeInternationalGames(_newSeasonDate.Year, _newSeasonDate);
        }

        private void ApplyEndOfSeasonDecisions()
        {
            _endOfSeasonChoicesService.HandleChoices(_saveDataService.SaveData.UserCountryId, GetUserAssociation());

            var developedClubs = _clubService.GetClubs()
                .Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId &&
                            c.LastSeasonFinancialResult != null);

            _gameUI.ShowClubDevelopment(
                GameUIMapper.ClubDevelopment(developedClubs), true);
            _gameUI.ShowSeasonEvent(GameUIMapper.Event(_seasonEconomyService.ApplyRandomEvent(GetUserAssociation())), true);
        }

        private void StartNextSeason()
        {
            _year++;
            _seasonService.Year = _year;

            var association = _footballAssociations
                .First(fa => fa.CountryId == _saveDataService.SaveData.UserCountryId);
            _seasonEconomyService.AllocateClubSubsidies(_saveDataService.SaveData.UserCountryId, _year, association);
            ConfigureSeasonPrizeMoney();

            _currentDate = new DateTime(_year, 7, 1);
            _newSeasonDate = _currentDate.AddYears(1);

            _clubsPerCompetition = _seasonService.InitializeNewSeason(_year);
            _strengthService.RecalculateCompetitionStrengths(_year);
            _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
            _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);
        }

        private void ConfigureSeasonPrizeMoney()
        {
            if (_playerCompetitionSettingsService.GetSettings().AutoPrizeMoney)
                return; 

            var competitions = _competitionService.GetCompetitions()
                .Where(c => c.CountryId == _saveDataService.SaveData.UserCountryId &&
                    (c.Type == CompetitionType.League || c.Type == CompetitionType.Cup))
                .OrderBy(c => c.Type).ThenBy(c => c.Tier).ToList();
            foreach (var competition in competitions)
            {
                if (competition.PrizeMoneySeasons.Any(s => s.Year == _year))
                    continue;
                while (true)
                {
                    var association = GetUserAssociation();
                    _gameUI.ShowMessage("Prijzengeld", $"Beschikbaar bondsbudget: {association.Balance:C2}", true);
                    var budget = _gameUI.AskPrizeMoneyBudget(competition.Name);
                    if (budget > 0 && budget > association.Balance)
                    {
                        _gameUI.ShowMessage("Onvoldoende budget", "Kies een lagere prijzenpot.", true);
                        continue;
                    }
                    var settings = new CompetitionPrizeMoneySettings
                    {
                        TotalBudget = budget,
                        Distribution = budget > 0
                            ? _gameUI.AskPrizeMoneyDistribution(competition.Type)
                            : competition.Type == CompetitionType.League
                                ? PrizeMoneyDistribution.Equal : PrizeMoneyDistribution.WinnerOnly
                    };
                    try
                    {
                        _endOfSeasonService.ReservePrizeMoney(competition, settings, association, _year);
                        break;
                    }
                    catch (InvalidOperationException ex)
                    {
                        _gameUI.ShowMessage("Prijzengeld", ex.Message, true);
                    }
                }
            }
        }

        private FootballAssociation GetUserAssociation()
        {
            return _footballAssociations.First(fa => fa.CountryId == _saveDataService.SaveData.UserCountryId);
        }

        private void ShowNews(DateTime date, Guid competitionId)
        {
            // Eén query, maar we vermijden dubbele enumeratie door te materializen als nodig
            var matches = _seasonService.NewsMessages.Where(nm =>
                nm.CountryId == _saveDataService.SaveData.UserCountryId &&
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

        private CountryInternationalCompetitionRanking DisplayInternationalRankingPerYear()
        {
            var ranking = _internationalRankingService.GetRanking(_year, Configuration.InternationalRankingYears);

            return ranking;
        }

        private List<Fixture> GetNextFixture(Guid competitionId, DateTime fromDate)
        {
            var nextDate = _fixtures
                .Where(fixture =>
                    fixture.CompetitionId == competitionId &&
                    fixture.MatchDay >= fromDate &&
                    (fixture.AwayTeam.CountryId == _saveDataService.SaveData.UserCountryId || fixture.HomeTeam.CountryId == _saveDataService.SaveData.UserCountryId))
                .Select(fixture => fixture.MatchDay)
                .OrderBy(date => date)
                .FirstOrDefault();

            if (nextDate == default || nextDate >= _newSeasonDate)
                return null;

            var fixtures = _fixtures.Where(f =>
                f.CompetitionId == competitionId &&
                f.MatchDay == nextDate &&
                (f.AwayTeam.CountryId == _saveDataService.SaveData.UserCountryId || f.HomeTeam.CountryId == _saveDataService.SaveData.UserCountryId));

            return fixtures.ToList();
        }

        private void ShowEndOfSeasonTables()
        {
            var leagueCompetitions = _competitionService.GetCompetitions()
                .Where(c => c.Type == CompetitionType.League)
                .OrderBy(c => c.CountryId == _saveDataService.SaveData.UserCountryId ? 0 : 1)
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
