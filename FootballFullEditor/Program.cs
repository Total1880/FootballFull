// See https://aka.ms/new-console-template for more information
using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using FootballFullEditor.ConsoleUI;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Services
services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ICountryService, CountryService>();
services.AddSingleton<ICompetitionService, CompetitionService>();
services.AddSingleton<IClubCompetitionService, ClubCompetitionService>();

// Repositories
const string dataRoot = @"C:\Users\olavh\source\repos\FootballFull\data";
services.AddSingleton<IRepository<Club>>(
    _ => new ClubRepositoryV2(Path.Combine(dataRoot, "Clubs.json")));

services.AddSingleton<IRepository<Competition>>(
    _ => new CompetitionRepositoryV2(Path.Combine(dataRoot, "Competitions.json")));

services.AddSingleton<IRepository<Country>>(
    _ => new CountryRepositoryV2(Path.Combine(dataRoot, "Countries.json")));

services.AddSingleton<IClubPerCompetitionRepository>(
    _ => new ClubPerCompetitionRepositoryV2(Path.Combine(dataRoot, "ClubPerCompetition.json")));

// Editors
services.AddSingleton<CountryEditor>();
services.AddSingleton<ClubEditor>();
services.AddSingleton<CompetitionEditor>();

var provider = services.BuildServiceProvider();

var clubEditor = provider.GetRequiredService<ClubEditor>();
var countryEditor = provider.GetRequiredService<CountryEditor>();
var competitionEditor = provider.GetRequiredService<CompetitionEditor>();



while (true)
{
    Console.Clear();
    Console.WriteLine("What to edit?");
    Console.WriteLine("\n[C]lubs  |  C[O]untry  |  [P]competitions  |  [Q]uit");

    var key = Console.ReadKey(intercept: true).Key;
    switch (key)
    {
        case ConsoleKey.C:
            clubEditor.Run();
            break;
        case ConsoleKey.O:
            countryEditor.Run();
            break;
        case ConsoleKey.P:
            competitionEditor.Run();
            break;
        case ConsoleKey.Q:
            return;
    }
}
