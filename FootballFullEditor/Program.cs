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

// Repositories
services.AddSingleton<IRepository<Club>, ClubRepositoryV2>();
services.AddSingleton<IRepository<Country>, CountryRepositoryV2>();

// Editors
services.AddSingleton<CountryEditor>();
services.AddSingleton<ClubEditor>();

var provider = services.BuildServiceProvider();

var clubEditor = provider.GetRequiredService<ClubEditor>();
var countryEditor = provider.GetRequiredService<CountryEditor>();

while (true)
{
    Console.Clear();
    Console.WriteLine("What to edit?");
    Console.WriteLine("\n[C]lubs  |  C[O]untry  |  [Q]uit");

    var key = Console.ReadKey(intercept: true).Key;
    switch (key)
    {
        case ConsoleKey.C:
            clubEditor.Run();
            break;
        case ConsoleKey.O:
            countryEditor.Run();
            break;
        case ConsoleKey.Q:
            return;
    }
}
