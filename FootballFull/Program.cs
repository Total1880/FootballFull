// See https://aka.ms/new-console-template for more information
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ISeasonService, SeasonService>();
services.AddSingleton<IFixtureService, FixtureService>();

services.AddSingleton<IClubRepository, ClubRepository>();

var provider = services.BuildServiceProvider();

var SeasonService = provider.GetRequiredService<ISeasonService>();
var FixtureService = provider.GetRequiredService<IFixtureService>();
var ClubService = provider.GetRequiredService<IClubService>();

SeasonService.InitializeNewSeason(ClubService.GetClubs());
var fixtures = FixtureService.Generate(ClubService.GetClubs());
var matchDays = fixtures.Max(_ => _.MatchDay);
Console.WriteLine("Football Season Fixtures:");
Console.WriteLine();
for (int matchDay = 1; matchDay <= matchDays; matchDay++)
{
    Console.WriteLine($"Match Day {matchDay}");
    var fixturesForMatchDay = fixtures.Where(_ => _.MatchDay == matchDay).ToList();
    foreach (var fixture in fixturesForMatchDay)
    {
        Console.WriteLine($"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
    }
    Console.WriteLine();
}

Console.ReadKey();
Console.Clear();

for (int matchDay = 1; matchDay <= matchDays; matchDay++)
{
    SeasonService.PlayMatchDay(fixtures, matchDay);
    Console.WriteLine($"Match Day {matchDay}");
    var fixturesForMatchDay = fixtures.Where(_ => _.MatchDay == matchDay).ToList();
    foreach (var fixture in fixturesForMatchDay)
    {
        Console.WriteLine($"{fixture.HomeTeam.Name} {fixture.HomeScore} vs {fixture.AwayTeam.Name} {fixture.AwayScore}");
    }
    Console.WriteLine();
    DisplayLeagueTable();
    Console.ReadKey();
    Console.Clear();
}

void DisplayLeagueTable()
{
    foreach (var clubLeagueCompetition in SeasonService.ClubLeagueCompetitions.OrderByDescending(_ => _.Points).ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst))
    {
        var club = ClubService.GetClubById(clubLeagueCompetition.ClubId);
        Console.WriteLine($"{club.Name} - Points: {clubLeagueCompetition.Points}, Goals For: {clubLeagueCompetition.GoalsFor}, Goals Against: {clubLeagueCompetition.GoalsAgainst}");
    }
}