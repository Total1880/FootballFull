using System;
using System.Collections.Generic;
using System.Linq;
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
        private Guid _userClubId;
        private int _matchDays;

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
        }

        public void Run()
        {
            // Data laden
            _clubsPerCompetition = _clubPerCompetitionService.GetAllClubPerCompetitions();
            _competitions = _competitionService.GetCompetitions();

            // Initialize data
            ResetStrength();
            CreateTrainers();

            // User club kiezen
            _userClubId = _seasonService.ChoosePlayerClub();

            // Eerste seizoen initialiseren
            _seasonService.Initialize(_clubsPerCompetition);
            _fixtures = _fixtureService.Generate(_clubsPerCompetition);
            _cupFixtures = _seasonService.InitializeNationalCups();
            _internationalFixtures = null;
            _matchDays = Configuration.Weeks;

            // Hoofdloop
            do
            {
                var competitionId = _clubsPerCompetition
                    .First(_ => _.ClubId == _userClubId)
                    .CompetitionId;

                var competitionToShow = _competitions.First(_ => _.Id == competitionId);

                Console.Clear();
                Console.WriteLine("=== Football Season Fixtures ===");
                Console.WriteLine();

                // Fixture overview
                DisplayLeagueTable();
                Console.WriteLine();
                DisplayNextFixture(competitionToShow, 0, waitForKey: false);

                Console.WriteLine("Press any key to start the season simulation...");
                Console.ReadKey();
                Console.Clear();

                for (int matchDay = 1; matchDay <= _matchDays; matchDay++)
                {
                    Console.Clear();
                    Console.WriteLine($"=== Matchday {matchDay} ===");

                    Console.WriteLine();
                    Console.WriteLine("Druk op een toets om deze speeldag te simuleren...");
                    Console.ReadKey();

                    _seasonService.PlayMatchDay(_fixtures, matchDay, false, _userClubId);

                    DisplayResult(matchDay);
                    Console.WriteLine();
                    DisplayLeagueTable();
                    Console.WriteLine();
                    DisplayNextFixture(competitionToShow, matchDay);
                    PlayCupGames(matchDay);
                    PlayInternationalGames(matchDay);

                    Console.Clear();

                    // 🔥 Nieuw: menu tussen speeldagen
                    ShowBetweenMatchdaysMenu();
                }

                // End of season
                Console.WriteLine("Season complete! Press any key to restart a new season.");
                Console.ReadKey();
                Console.Clear();

                _internationalFixtures = _seasonService.InitializeInternationalGames();
                _seasonService.InitializeNewSeason();
                _fixtures = _fixtureService.Generate(_clubsPerCompetition);
                _cupFixtures = _seasonService.InitializeNationalCups();

            } while (true);
        }

        #region Helpers

        private void ShowBetweenMatchdaysMenu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Volgende speeldag");
                Console.WriteLine("2. Andere lopende competities bekijken");
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

                    case ConsoleKey.NumPad0:
                        Environment.Exit(0);
                        return;

                    default:
                        Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
                        break;
                }
            }
        }

        private void PlayCupGames(int matchDay)
        {
            if (_cupFixtures == null || !_cupFixtures.Any())
                return;
            if (!_cupFixtures.Any(_ => _.MatchDay == matchDay))
                return;

            var cupCompetitions = _competitions
                .Where(_ => _.Type == Competition.CompetitionType.Cup)
                .ToList();

            Console.Clear();
            Console.WriteLine("=== National Cup Fixtures ===");
            Console.WriteLine();

            foreach (var cupCompetition in cupCompetitions)
            {
                Console.WriteLine($"--- {cupCompetition.Name} ---");
                var fixturesForCompetition = _cupFixtures
                    .Where(_ => _.CompetitionId == cupCompetition.Id && _.MatchDay == matchDay)
                    .ToList();

                if (fixturesForCompetition.Count == 0)
                    continue;

                Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} ===");

                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId == Guid.Empty || fixture.AwayTeamId == Guid.Empty)
                        continue;

                    if (fixture.HomeTeamId == _userClubId || fixture.AwayTeamId == _userClubId)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    var homeTier = GetClubTier(fixture.HomeTeamId);
                    var awayTier = GetClubTier(fixture.AwayTeamId);

                    Console.WriteLine(
                        $"{fixture.HomeTeam.Name} ({homeTier}) vs {fixture.AwayTeam.Name} ({awayTier})"
                    );

                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to play this round...");
                Console.ReadKey();

                // Speel enkel deze ronde
                _seasonService.PlayMatchDay(fixturesForCompetition, matchDay, true, _userClubId);
                Console.Clear();
                Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} Results ===");

                foreach (var fixture in fixturesForCompetition)
                {
                    if (fixture.HomeTeamId != Guid.Empty && fixture.AwayTeamId != Guid.Empty)
                    {

                        if (fixture.HomeTeamId == _userClubId || fixture.AwayTeamId == _userClubId)
                            Console.ForegroundColor = ConsoleColor.Yellow;

                        var homeTier = GetClubTier(fixture.HomeTeamId);
                        var awayTier = GetClubTier(fixture.AwayTeamId);

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

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void PlayInternationalGames(int matchDay)
        {
            if (_internationalFixtures == null || !_internationalFixtures.Any())
                return;
            if (!_internationalFixtures.Any(_ => _.MatchDay == matchDay))
                return;

            var fixturesForRound = _internationalFixtures
                .Where(_ => _.MatchDay == matchDay)
                .ToList();

            if (fixturesForRound.Count == 0)
                return;

            Console.Clear();
            Console.WriteLine($"=== International Round {matchDay} ===");
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
            Console.WriteLine("Press any key to start the international cup...");
            Console.ReadKey();
            Console.Clear();

            Console.WriteLine($"=== International Round {matchDay} ===");
            foreach (var fixture in fixturesForRound)
            {
                Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to play this round...");
            Console.ReadKey();

            _seasonService.PlayMatchDay(fixturesForRound, matchDay, true, _userClubId);

            Console.Clear();
            Console.WriteLine($"=== International Round {matchDay} Results ===");
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

            if (final.MatchDay != matchDay)
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

        private void DisplayResult(int matchDay)
        {
            var competitionId = _clubsPerCompetition
                .First(_ => _.ClubId == _userClubId)
                .CompetitionId;

            var competitionToShow = _competitions.First(_ => _.Id == competitionId);

            Console.WriteLine();
            Console.WriteLine($"=== Match Day {matchDay} - {competitionToShow.Name} ===");
            Console.WriteLine();

            var fixturesForMatchDay = _fixtures
                .Where(_ => _.MatchDay == matchDay && _.CompetitionId == competitionToShow.Id)
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

        private void DisplayNextFixture(Competition competitionToShow, int matchDay, bool waitForKey = true)
        {
            if (matchDay < _matchDays)
            {
                var nextDay = matchDay + 1;
                Console.WriteLine($"Next Match Day: {nextDay}");
                Console.WriteLine(new string('-', 25));

                var nextFixtures = _fixtures
                    .Where(_ => _.MatchDay == nextDay && _.CompetitionId == competitionToShow.Id)
                    .ToList();

                if (nextFixtures.Count == 0)
                {
                    Console.WriteLine("No fixtures available.");
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
                    Console.WriteLine("Press any key for next matchday...");
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

        private void CreateTrainers() {
            if (_trainers == null || _trainers.Count() == 0)
            {
                _trainers = new List<Trainer>();
                foreach (var club in _clubService.GetClubs())
                {
                    var trainer = new Trainer
                    {
                        Id = new Guid(),
                        ClubId = club.Id,
                        Motivation = Random.Shared.Next(0, 5),
                        TacticalSkill = Random.Shared.Next(0, 5),
                    };

                    _trainers.Add(trainer);
                }

                _trainerService.CreateTrainers(_trainers);
            }
        }
        #endregion
    }
}
