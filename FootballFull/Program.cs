// See https://aka.ms/new-console-template for more information
using FootballFull.Services;



var SeasonService = new SeasonService();
var FixtureService = new FixtureService();
var ClubService = new ClubService();

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
    Console.ReadKey();
    Console.Clear();
}
