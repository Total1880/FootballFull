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
services.AddSingleton<IClubPerCompetitionService, ClubPerCompetitionService>();
services.AddSingleton<ICompetitionService, CompetitionService>();

services.AddSingleton<IClubRepository, ClubRepository>();
services.AddSingleton<IClubPerCompetitionRepository, ClubPerCompetitionRepository>();
services.AddSingleton<ICompetitionRepository, CompetitionRepository>();
services.AddSingleton<ICountryRepository, CountryRepository>();

var provider = services.BuildServiceProvider();

var SeasonService = provider.GetRequiredService<ISeasonService>();
var FixtureService = provider.GetRequiredService<IFixtureService>();
var ClubService = provider.GetRequiredService<IClubService>();
var ClubPerCompetitionService = provider.GetRequiredService<IClubPerCompetitionService>();
var CompetitionService = provider.GetRequiredService<ICompetitionService>();

var clubsPerCompetition = ClubPerCompetitionService.GetClubsPerCompetition();
SeasonService.Initialize(clubsPerCompetition);
var fixtures = FixtureService.Generate(clubsPerCompetition);
var matchDays = fixtures.Max(_ => _.MatchDay);
do
{
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
        DisplayResult(matchDay);
        Console.WriteLine();
        DisplayLeagueTable();
        Console.ReadKey();
        Console.Clear();
    }

    Console.WriteLine("Season complete! Press any key to restart a new season.");
    Console.ReadKey();
    Console.Clear();

    SeasonService.InitializeNewSeason();
    fixtures = FixtureService.Generate(clubsPerCompetition);

} while (true);

void DisplayLeagueTable()
{
    // Bepaal kolombreedtes (optioneel dynamisch)
    const int nameWidth = 25;
    const int pointsWidth = 8;
    const int gfWidth = 10;
    const int gaWidth = 12;

    foreach (var competition in CompetitionService.GetCompetitions())
    {
        // Header
        Console.WriteLine(
        $"{"Club".PadRight(nameWidth)}" +
        $"{"Points".PadLeft(pointsWidth)}" +
        $"{"GF".PadLeft(gfWidth)}" +
        $"{"GA".PadLeft(gaWidth)}"
    );

        Console.WriteLine(new string('-', nameWidth + pointsWidth + gfWidth + gaWidth));

        foreach (var c in SeasonService.ClubLeagueCompetitions
            .Where(_ => _.CompetitionId == competition.Id)
            .OrderByDescending(_ => _.Points)
            .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst))
        {
            var club = ClubService.GetClubById(c.ClubId);

            Console.WriteLine(
                $"{club.Name.PadRight(nameWidth)}" +
                $"{c.Points.ToString().PadLeft(pointsWidth)}" +
                $"{c.GoalsFor.ToString().PadLeft(gfWidth)}" +
                $"{c.GoalsAgainst.ToString().PadLeft(gaWidth)}"
            );
        }
    }
}

void DisplayResult(int matchDay)
{
    Console.WriteLine();
    Console.WriteLine($"=== Match Day {matchDay} ===");
    Console.WriteLine();

    // Haal alle fixtures
    foreach (var competition in CompetitionService.GetCompetitions())
    {
        Console.WriteLine($"--- {competition.Name} ---");
        Console.WriteLine();

        var fixturesForMatchDay = fixtures
            .Where(_ => _.MatchDay == matchDay && _.CompetitionId == competition.Id)
            .ToList();

        if (fixturesForMatchDay.Count == 0) continue;

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

}