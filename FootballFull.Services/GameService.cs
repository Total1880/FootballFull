using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;

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

        private IList<ClubPerCompetition> _clubsPerCompetition = new List<ClubPerCompetition>();
        private IList<Competition> _competitions = new List<Competition>();
        private IList<Fixture> _fixtures = new List<Fixture>();
        private IList<Fixture> _cupFixtures = new List<Fixture>();
        private IList<Trainer> _trainers;
        private IList<Fixture>? _internationalFixtures;
        private IList<NewsMessage> _news;
        private Guid _userClubId;
        private DateTime _currentDate;
        private DateTime _newSeasonDate;
        private int _year = DateTime.Now.Year;

        public GameService(
            ISeasonService seasonService,
            IFixtureService fixtureService,
            IClubService clubService,
            ICompetitionService competitionService,
            IClubPerCompetitionService clubPerCompetitionService,
            ICountryService countryService,
            ITrainerService trainerService)
        {
            _seasonService = seasonService;
            _fixtureService = fixtureService;
            _clubService = clubService;
            _competitionService = competitionService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _countryService = countryService;
            _trainerService = trainerService;

            _trainers = _trainerService.Load();
            _news = new List<NewsMessage>();
        }

        public void Run(bool isNew)
        {
            // Data laden
            _clubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            _competitions = _competitionService.GetCompetitions();
            _currentDate = new DateTime(_year, 7, 1);
            _newSeasonDate = _currentDate.AddYears(1);

            // Initialize data
            if (isNew)
            {
                ResetStrength();
                CreateTrainers();
                _internationalFixtures = null;
            }

            // User club kiezen
            _userClubId = _seasonService.ChoosePlayerClub();

            // Eerste seizoen initialiseren
            _seasonService.Initialize(_clubsPerCompetition);
            _year = _seasonService.Year;
            _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
            _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);


            if (!isNew)
            {
                _internationalFixtures = _seasonService.InitializeInternationalGames(_currentDate, true);
            }

            // Hoofdloop
            do
            {
                var competitionId = _clubsPerCompetition
                    .First(_ => _.ClubId == _userClubId)
                    .CompetitionId;

                var competitionToShow = _competitions.First(_ => _.Id == competitionId);

                Console.Clear();

                // Fixture overview
                DisplayLeagueTable();
                Console.WriteLine();
                DisplayNextFixture(competitionToShow, _currentDate, waitForKey: false);

                Console.WriteLine("Press any key to start the season simulation...");
                Console.ReadKey();
                Console.Clear();

                do
                {
                    ShowBetweenMatchdaysMenu();
                    Console.Clear();
                    Console.WriteLine($"=== Date {_currentDate:dddd dd/MM/yyyy} ===");

                    Console.WriteLine();
                    Console.WriteLine("Druk op een toets om deze dag te simuleren...");
                    Console.ReadKey();

                    _seasonService.PlayMatchDay(_fixtures, _currentDate, false, _userClubId);

                    DisplayResult(_currentDate);
                    Console.WriteLine();
                    DisplayLeagueTable();
                    Console.WriteLine();
                    DisplayNextFixture(competitionToShow, _currentDate);
                    PlayCupGames(_currentDate);
                    PlayInternationalGames(_currentDate);
                    _seasonService.UpdateWeekStats(_userClubId, _currentDate);

                    Console.Clear();

                    ShowNews(_currentDate, competitionId);
                    _currentDate = _currentDate.AddDays(1);
                } while (_currentDate < _newSeasonDate);

                // End of season
                Console.WriteLine("Season complete! Press any key to restart a new season.");
                Console.ReadKey();
                Console.Clear();

                _internationalFixtures = _seasonService.InitializeInternationalGames(_currentDate);
                _seasonService.InitializeNewSeason(_year);
                _fixtures = _fixtureService.Generate(_clubsPerCompetition, _currentDate);
                _cupFixtures = _seasonService.InitializeNationalCups(_currentDate);

                _seasonService.SaveGame();

            } while (true);
        }

        private void ShowNews(DateTime date, Guid competitionId)
        {
            Console.Clear();

            var club = _clubService.GetClubById(_userClubId);
            var countryId = club.CountryId;

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

                var input = Console.ReadKey();

                switch (input.Key)
                {
                    case ConsoleKey.NumPad1:
                        // Terug naar de for-loop: volgende matchday
                        return;

                    case ConsoleKey.NumPad2:
                        ShowOtherCompetitionsMenu(); // hieronder
                                                     // Na terugkeer tonen we opnieuw dit menu
                        break;
                    case ConsoleKey.NumPad3:
                        ClubMenu();
                        break;
                    case ConsoleKey.NumPad4:
                        Console.WriteLine("Not implemented yet");
                        break;
                    case ConsoleKey.NumPad5:
                        DisplayInternationalRankingPerYear();
                        break;

                    case ConsoleKey.NumPad0:
                        Environment.Exit(0);
                        return;

                    default:
                        Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
                        break;
                }
            }
        }

        private void ClubMenu()
        {
            Console.Clear();
            Console.WriteLine();
            var trainer = _seasonService.UserTrainer(_userClubId);
            Console.WriteLine($"Trainer: {trainer?.Name} {trainer?.LastName}");
#if DEBUG
            Console.WriteLine($"Tactical: {trainer?.TacticalSkill}");
            Console.WriteLine($"Motivational: {trainer?.Motivation}");
#endif
            Console.WriteLine("Trainer ontslagen? (Y/N)");
            var input = Console.ReadKey();
            switch (input.Key)
            {
                case ConsoleKey.Y:
                    _seasonService.NewTrainer(_userClubId, _currentDate);
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

            var userCountry = _clubService.GetClubById(_userClubId).CountryId;
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

                    if (fixture.HomeTeamId == _userClubId || fixture.AwayTeamId == _userClubId)
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
                _seasonService.PlayMatchDay(fixturesForCompetition, date, true, _userClubId, true);
                if (display)
                {
                    Console.Clear();
                    Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} Results ===");
                }
                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId != Guid.Empty && fixture.AwayTeamId != Guid.Empty)
                    {

                        if (fixture.HomeTeamId == _userClubId || fixture.AwayTeamId == _userClubId)
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

            _seasonService.PlayMatchDay(fixturesForRound, date, true, _userClubId, true);

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

        private void DisplayLeagueTable()
        {
            // Bepaal competitie van de user
            var competitionId = _clubsPerCompetition
                .First(_ => _.ClubId == _userClubId)
                .CompetitionId;

            // User-club highlighten
            DisplayLeagueTable(competitionId, _userClubId);
        }

        private void DisplayLeagueTable(Guid competitionId, Guid? highlightClubId = null)
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
        }

        private void DisplayResult(DateTime date)
        {
            var competitionId = _clubsPerCompetition
                .First(_ => _.ClubId == _userClubId)
                .CompetitionId;

            var competitionToShow = _competitions.First(_ => _.Id == competitionId);

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
                if (fixture.HomeTeamId == _userClubId || fixture.AwayTeamId == _userClubId)
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

        private void DisplayNextFixture(Competition competitionToShow, DateTime date, bool waitForKey = true)
        {
            if (date < _newSeasonDate)
            {
                Console.WriteLine($"Next day: {date.AddDays(1)}");
                Console.WriteLine(new string('-', 25));

                var nextFixtures = _fixtures
                    .Where(_ => _.MatchDay == date.AddDays(1) && _.CompetitionId == competitionToShow.Id)
                    .ToList();

                if (nextFixtures.Count == 0)
                {
                    Console.WriteLine("No fixtures available.");
                    if (waitForKey)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Press any key for next day...");
                        Console.ReadKey();
                    }
                    return;
                }

                foreach (var f in nextFixtures)
                {
                    if (f.HomeTeamId == _userClubId || f.AwayTeamId == _userClubId)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine($"{f.HomeTeam.Name} vs {f.AwayTeam.Name}");
                    Console.ResetColor();
                }

                if (waitForKey)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key for next week...");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Season complete! Press any key...");
                Console.ReadKey();
            }
        }

        private void ResetStrength()
        {
            var countries = _countryService.GetCountries();
            var clubs = _clubService.GetClubs();

            foreach (var country in countries)
            {
                ResetClubStrength(clubs, country);
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
                club.Strength = currentStrength > 0 ? currentStrength : 1;
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
                .Where(_ => _.CompetitionId == competitionId && _.HomeScore >= 0)
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
        #endregion
    }
}
