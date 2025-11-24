// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OlavFramework;

var services = new ServiceCollection();

// Services
services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ISeasonService, SeasonService>();
services.AddSingleton<IFixtureService, FixtureService>();
services.AddSingleton<ICompetitionService, CompetitionService>();
services.AddSingleton<IClubPerCompetitionService, ClubPerCompetitionService>();
services.AddSingleton<ICountryService, CountryService>();

// Repositories (V2-varianten)
const string dataRoot = @"C:\Users\olavh\source\repos\FootballFull\data";
services.AddSingleton<IRepository<Club>>(
    _ => new ClubRepositoryV2(Path.Combine(dataRoot, "Clubs.json")));

services.AddSingleton<IRepository<Competition>>(
    _ => new CompetitionRepositoryV2(Path.Combine(dataRoot, "Competitions.json")));

services.AddSingleton<IRepository<Country>>(
    _ => new CountryRepositoryV2(Path.Combine(dataRoot, "Countries.json")));

services.AddSingleton<IClubPerCompetitionRepository>(
    _ => new ClubPerCompetitionRepositoryV2(Path.Combine(dataRoot, "ClubPerCompetition.json")));

var provider = services.BuildServiceProvider();

// Resolve services
var seasonService = provider.GetRequiredService<ISeasonService>();
var fixtureService = provider.GetRequiredService<IFixtureService>();
var clubService = provider.GetRequiredService<IClubService>();
var competitionService = provider.GetRequiredService<ICompetitionService>();
var clubsPerCompetitionService = provider.GetRequiredService<IClubPerCompetitionService>();
var countryService = provider.GetRequiredService<ICountryService>();

// Data laden
var clubsPerCompetition = clubsPerCompetitionService.GetAllClubPerCompetitions();
var competitions = competitionService.GetCompetitions();

// reset strength
ResetStrenght();

// TODO: user club Id kiezen via config / input
Guid userClubId = seasonService.ChoosePlayerClub();

// Init eerste seizoen
seasonService.Initialize(clubsPerCompetition);
var fixtures = fixtureService.Generate(clubsPerCompetition);
var cupFixtures = seasonService.InitializeNationalCups();
var internationalFixtures = seasonService.InitializeInternationalGames();
var matchDays = Configuration.Weeks;

do
{
    var competitionId = clubsPerCompetition
.First(_ => _.ClubId == userClubId)
.CompetitionId;

    var competitionToShow = competitions.First(_ => _.Id == competitionId);

    Console.Clear();
    Console.WriteLine("=== Football Season Fixtures ===");
    Console.WriteLine();

    // --- FIXTURE OVERVIEW ---
    DisplayLeagueTable();
    Console.WriteLine();
    DisplayNextFixture(userClubId, fixtures, matchDays, competitionToShow, 0);

    Console.WriteLine("Press any key to start the season simulation...");
    Console.ReadKey();
    Console.Clear();

    // --- MATCHDAY SIMULATION ---
    for (int matchDay = 1; matchDay <= matchDays; matchDay++)
    {
        seasonService.PlayMatchDay(fixtures, matchDay, false, userClubId);

        // Toon resultaten van speeldag
        DisplayResult(matchDay);
        Console.WriteLine();
        DisplayLeagueTable();
        Console.WriteLine();
        DisplayNextFixture(userClubId, fixtures, matchDays, competitionToShow, matchDay);
        PlayCupGames(matchDay);
        PlayInternationalGames(matchDay);

        Console.Clear();
    }

    // --- END OF SEASON ---

    Console.WriteLine("Season complete! Press any key to restart a new season.");
    Console.ReadKey();
    Console.Clear();

    seasonService.InitializeNewSeason();
    fixtures = fixtureService.Generate(clubsPerCompetition);
    cupFixtures = seasonService.InitializeNationalCups();
    internationalFixtures = seasonService.InitializeInternationalGames();
} while (true);

void PlayCupGames(int matchDay)
{
    if (cupFixtures == null || !cupFixtures.Any())
        return;
    if( cupFixtures.Any(_ => _.MatchDay == matchDay) == false)
        return;

    var cupCompetitions = competitions
    .Where(_ => _.Type == Competition.CompetitionType.Cup).ToList();

    Console.Clear();
    Console.WriteLine("=== National Cup Fixtures ===");
    Console.WriteLine();

    foreach (var cupCompetition in cupCompetitions)
    {
        Console.WriteLine($"--- {cupCompetition.Name} ---");
        var fixturesForCompetition = cupFixtures
            .Where(_ => _.CompetitionId == cupCompetition.Id && _.MatchDay == matchDay)
            .ToList();

            Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} ===");
            foreach (var fixture in fixturesForCompetition)
            {
                if (fixture.HomeTeamId == userClubId || fixture.AwayTeamId == userClubId)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
                Console.ResetColor();
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to play this round...");
            Console.ReadKey();
            // Simuleer enkel deze ronde (MatchDay == round)
            seasonService.PlayMatchDay(fixturesForCompetition, matchDay, true, userClubId);
            Console.Clear();
            Console.WriteLine($"=== {cupCompetition.Name} Round {fixturesForCompetition.First().RoundNo} Results ===");
            foreach (var fixture in fixturesForCompetition)
            {
                if (fixture.HomeTeamId == userClubId || fixture.AwayTeamId == userClubId)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{fixture.HomeTeam.Name} {fixture.HomeScore} - {fixture.AwayScore} {fixture.AwayTeam.Name}");
                Console.ResetColor();

            //update winners for next round
            var winner = fixture.HomeScore > fixture.AwayScore ? fixture.HomeTeam : fixture.AwayTeam;
            var cupNextFixtures = cupFixtures.FirstOrDefault(_ => _.CupPreviousFixtureHomeTeam == fixture);
            if (cupNextFixtures != null)
            {
                if (cupNextFixtures.HomeTeamId == Guid.Empty)
                {
                    cupNextFixtures.HomeTeam = winner;
                    cupNextFixtures.HomeTeamId = winner.Id;
                }
            } else
            {
                cupNextFixtures = cupFixtures.FirstOrDefault(_ => _.CupPreviousFixtureAwayTeam == fixture);
                if (cupNextFixtures != null && cupNextFixtures.AwayTeamId == Guid.Empty)
                {
                    cupNextFixtures.AwayTeam = winner;
                    cupNextFixtures.AwayTeamId = winner.Id;
                }
            }
            Console.WriteLine();
            Console.Clear();
        }
    }
}

void PlayInternationalGames(int matchDay)
{
    if (internationalFixtures == null || !internationalFixtures.Any())
        return;
    if (internationalFixtures.Any(_ => _.MatchDay == matchDay) == false)
        return;

    var internationalCompetition = competitions
        .First(_ => _.Type == Competition.CompetitionType.International);

    Console.Clear();
    Console.WriteLine("=== International Cup Fixtures ===");
    Console.WriteLine();

    var maxRound = internationalFixtures.Max(_ => _.RoundNo);

    // Initieel overzicht (met TBD/winnaars uit vorige matchen)
    for (int round = 0; round <= maxRound; round++)
    {
        Console.WriteLine($"Round {round}");
        var fixturesForRound = internationalFixtures
            .Where(_ => _.RoundNo == round)
            .ToList();

        foreach (var fixture in fixturesForRound)
        {
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
    }

    Console.WriteLine("Press any key to start the international cup...");
    Console.ReadKey();
    Console.Clear();

    // Ronde per ronde spelen
    for (int round = 0; round <= maxRound; round++)
    {
        var fixturesForRound = internationalFixtures
            .Where(_ => _.RoundNo == round)
            .ToList();

        // Winners uit vorige ronde invullen in deze ronde
        if (round > 0)
        {
            foreach (var fixture in fixturesForRound)
            {
                if (fixture.CupPreviousFixtureHomeTeam != null && fixture.HomeTeamId == Guid.Empty)
                {
                    var prev = fixture.CupPreviousFixtureHomeTeam;
                    var homeWinner = prev.HomeScore > prev.AwayScore ? prev.HomeTeam : prev.AwayTeam;

                    fixture.HomeTeam = homeWinner;
                    fixture.HomeTeamId = homeWinner.Id;
                }

                if (fixture.CupPreviousFixtureAwayTeam != null && fixture.AwayTeamId == Guid.Empty)
                {
                    var prev = fixture.CupPreviousFixtureAwayTeam;
                    var awayWinner = prev.HomeScore > prev.AwayScore ? prev.HomeTeam : prev.AwayTeam;

                    fixture.AwayTeam = awayWinner;
                    fixture.AwayTeamId = awayWinner.Id;
                }
            }
        }

        // MatchDay gelijk zetten aan RoundNo zodat PlayMatchDay ze kan oppikken
        foreach (var fixture in fixturesForRound)
        {
            fixture.MatchDay = round;
        }

        Console.WriteLine($"=== International Round {round} ===");
        foreach (var fixture in fixturesForRound)
        {
            Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to play this round...");
        Console.ReadKey();

        // Simuleer enkel deze ronde (MatchDay == round)
        seasonService.PlayMatchDay(internationalFixtures, round, true, userClubId);

        Console.Clear();
        Console.WriteLine($"=== International Round {round} Results ===");
        foreach (var fixture in fixturesForRound)
        {
            Console.WriteLine($"{fixture.HomeTeam.Name} {fixture.HomeScore} - {fixture.AwayScore} {fixture.AwayTeam.Name}");
        }

        Console.WriteLine();
        if (round < maxRound)
        {
            Console.WriteLine("Press any key for next round...");
            Console.ReadKey();
        }
        else if (round == maxRound)
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        Console.Clear();
    }

    // Winnaar bepalen – laatste ronde
    var final = internationalFixtures
        .OrderByDescending(f => f.RoundNo)
        .First();

    var winner = final.HomeScore > final.AwayScore ? final.HomeTeam : final.AwayTeam;
    Console.WriteLine($"International Cup winner: {winner.Name}!");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}



static void ShowAllFixtures(IList<Fixture> fixtures, int matchDays)
{
    for (int matchDay = 1; matchDay <= matchDays; matchDay++)
    {
        Console.WriteLine($"Match Day {matchDay}");
        Console.WriteLine(new string('-', 22));

        var fixturesForMatchDay = fixtures
            .Where(_ => _.MatchDay == matchDay)
            .ToList();

        foreach (var fixture in fixturesForMatchDay)
        {
            Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
        }

        Console.WriteLine();
    }
}


void DisplayLeagueTable()
{
    // Bepaal kolombreedtes
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

    // Bepaal competitie van user club
    var competitionId = clubsPerCompetition
        .First(_ => _.ClubId == userClubId)
        .CompetitionId;

    var competitionToShow = competitions.First(_ => _.Id == competitionId);

    Console.WriteLine($"=== League Table: {competitionToShow.Name} ===");
    Console.WriteLine();

    // Header
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
    foreach (var c in seasonService.ClubLeagueCompetitions
                 .Where(_ => _.CompetitionId == competitionToShow.Id)
                 .OrderByDescending(_ => _.Points)
                 .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst))
    {
        var club = clubService.GetClubById(c.ClubId);
        if (c.ClubId == userClubId)
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

void DisplayResult(int matchDay)
{
    var competitionId = clubsPerCompetition
        .First(_ => _.ClubId == userClubId)
        .CompetitionId;

    var competitionToShow = competitions.First(_ => _.Id == competitionId);

    Console.WriteLine();
    Console.WriteLine($"=== Match Day {matchDay} - {competitionToShow.Name} ===");
    Console.WriteLine();

    var fixturesForMatchDay = fixtures
        .Where(_ => _.MatchDay == matchDay && _.CompetitionId == competitionToShow.Id)
        .ToList();

    if (fixturesForMatchDay.Count == 0)
        return;

    // Dynamische kolombreedtes bepalen
    int homeWidth = fixturesForMatchDay.Max(f => f.HomeTeam.Name.Length) + 2;
    int awayWidth = fixturesForMatchDay.Max(f => f.AwayTeam.Name.Length) + 2;

    // Header
    Console.WriteLine(
        $"{"Home Team".PadRight(homeWidth)}" +
        $"{"Score".PadRight(8)}" +
        $"{"Away Team".PadRight(awayWidth)}"
    );

    Console.WriteLine(new string('-', homeWidth + 8 + awayWidth));

    foreach (var fixture in fixturesForMatchDay)
    {
        var score = $"{fixture.HomeScore} - {fixture.AwayScore}";
        if (fixture.HomeTeamId == userClubId || fixture.AwayTeamId == userClubId)
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

static void DisplayNextFixture(Guid userClubId, IList<Fixture> fixtures, int matchDays, Competition competitionToShow, int matchDay)
{
    if (matchDay < matchDays)
    {
        var nextDay = matchDay + 1;
        Console.WriteLine($"Next Match Day: {nextDay}");
        Console.WriteLine(new string('-', 25));

        var nextFixtures = fixtures
            .Where(_ => _.MatchDay == nextDay && _.CompetitionId == competitionToShow.Id)
            .ToList();

        if (nextFixtures.Count == 0)
        {
            Console.WriteLine("No fixtures available.");
            return;
        }

        foreach (var f in nextFixtures)
        {
            if (f.HomeTeamId == userClubId || f.AwayTeamId == userClubId)
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{f.HomeTeam.Name} vs {f.AwayTeam.Name}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key for next matchday...");
        Console.ReadKey();
    }
    else
    {
        // Laatste speeldag → geen preview
        Console.WriteLine("Season complete! Press any key...");
        Console.ReadKey();
    }
}

void ResetStrenght()
{

    var countries = countryService.GetCountries();
    var clubs = clubService.GetClubs();

    foreach (var country in countries)
    {
        var clubsInCountry = clubs
            .Where(c => c.CountryId == country.Id)
            .ToList();

        var counter = clubsInCountry.Count / (Configuration.MaxStrength - Configuration.MinStrength);
        var currentStrength = Configuration.MaxStrength;

        for (int i = 0; i < clubsInCountry.Count; i++)
        {
            var club = clubsInCountry[i];
            club.Strength = currentStrength > 0 ? currentStrength : 1;
            clubService.Update(club);

            counter--;
            if (counter <= 0)
            {
                currentStrength--;
                counter = clubsInCountry.Count / (Configuration.MaxStrength - Configuration.MinStrength);
            }
        }
    }
}