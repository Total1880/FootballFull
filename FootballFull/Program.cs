// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Services
services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ISeasonService, SeasonService>();
services.AddSingleton<IFixtureService, FixtureService>();
services.AddSingleton<ICompetitionService, CompetitionService>();
services.AddSingleton<IClubCompetitionService, ClubCompetitionService>();

// Repositories (V2-varianten)
const string dataRoot = @"C:\Users\olavh\source\repos\FootballFull\data";
services.AddSingleton<IRepository<Club>>(
    _ => new ClubRepositoryV2(Path.Combine(dataRoot, "Clubs.json")));

services.AddSingleton<IRepository<Competition>>(
    _ => new CompetitionRepositoryV2(Path.Combine(dataRoot, "Competitions.json")));

services.AddSingleton<IRepository<Country>>(
    _ => new CountryRepositoryV2(Path.Combine(dataRoot, "Countries.json")));

services.AddSingleton<IRepository<ClubPerCompetition>>(
    _ => new ClubPerCompetitionRepositoryV2(Path.Combine(dataRoot, "ClubPerCompetition.json")));

var provider = services.BuildServiceProvider();

// Resolve services
var seasonService = provider.GetRequiredService<ISeasonService>();
var fixtureService = provider.GetRequiredService<IFixtureService>();
var clubService = provider.GetRequiredService<IClubService>();
var competitionService = provider.GetRequiredService<ICompetitionService>();
var clubPerCompetitionRepo = provider.GetRequiredService<IRepository<ClubPerCompetition>>();

// Data laden
var clubsPerCompetition = clubPerCompetitionRepo.Load();
var competitions = competitionService.GetCompetitions();

// TODO: user club Id kiezen via config / input
Guid userClubId = new Guid("5f5232b8-6969-4e41-ab84-ce363f64922e");

// Init eerste seizoen
seasonService.Initialize(clubsPerCompetition);
var fixtures = fixtureService.Generate(clubsPerCompetition);
var matchDays = fixtures.Max(_ => _.MatchDay);

do
{
    Console.Clear();
    Console.WriteLine("Football Season Fixtures:");
    Console.WriteLine();

    // Alle fixtures tonen
    for (int matchDay = 1; matchDay <= matchDays; matchDay++)
    {
        Console.WriteLine($"Match Day {matchDay}");
        var fixturesForMatchDay = fixtures
            .Where(_ => _.MatchDay == matchDay)
            .ToList();

        foreach (var fixture in fixturesForMatchDay)
        {
            Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
        }

        Console.WriteLine();
    }

    Console.WriteLine("Press any key to start the season simulation...");
    Console.ReadKey();
    Console.Clear();

    // Matchdays spelen
    for (int matchDay = 1; matchDay <= matchDays; matchDay++)
    {
        seasonService.PlayMatchDay(fixtures, matchDay, userClubId);

        DisplayResult(matchDay);
        Console.WriteLine();
        DisplayLeagueTable();

        Console.WriteLine("Press any key for next matchday...");
        Console.ReadKey();
        Console.Clear();
    }

    Console.WriteLine("Season complete! Press any key to restart a new season.");
    Console.ReadKey();
    Console.Clear();

    seasonService.InitializeNewSeason();
    fixtures = fixtureService.Generate(clubsPerCompetition);
    matchDays = fixtures.Max(_ => _.MatchDay);

} while (true);

void DisplayLeagueTable()
{
    // Bepaal kolombreedtes
    const int nameWidth = 25;
    const int pointsWidth = 8;
    const int gfWidth = 10;
    const int gaWidth = 12;

    // Bepaal competitie van user club
    var competitionId = clubsPerCompetition
        .First(_ => _.ClubId == userClubId)
        .CompetitionId;

    var competitionToShow = competitions.First(_ => _.Id == competitionId);

    Console.WriteLine($"=== League Table: {competitionToShow.Name} ===");
    Console.WriteLine();

    // Header
    Console.WriteLine(
        $"{"Club".PadRight(nameWidth)}" +
        $"{"Points".PadLeft(pointsWidth)}" +
        $"{"GF".PadLeft(gfWidth)}" +
        $"{"GA".PadLeft(gaWidth)}"
    );

    Console.WriteLine(new string('-', nameWidth + pointsWidth + gfWidth + gaWidth));

    foreach (var c in seasonService.ClubLeagueCompetitions
                 .Where(_ => _.CompetitionId == competitionToShow.Id)
                 .OrderByDescending(_ => _.Points)
                 .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst))
    {
        var club = clubService.GetClubById(c.ClubId);

        Console.WriteLine(
            $"{club.Name.PadRight(nameWidth)}" +
            $"{c.Points.ToString().PadLeft(pointsWidth)}" +
            $"{c.GoalsFor.ToString().PadLeft(gfWidth)}" +
            $"{c.GoalsAgainst.ToString().PadLeft(gaWidth)}"
        );
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

        Console.WriteLine(
            $"{fixture.HomeTeam.Name.PadRight(homeWidth)}" +
            $"{score.PadRight(8)}" +
            $"{fixture.AwayTeam.Name.PadRight(awayWidth)}"
        );
    }

    Console.WriteLine();
}
