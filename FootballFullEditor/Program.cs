// See https://aka.ms/new-console-template for more information
using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ICountryService, CountryService>();

services.AddSingleton<IRepository<Club>, ClubRepositoryV2>();
services.AddSingleton<IRepository<Country>, CountryRepositoryV2>();

var provider = services.BuildServiceProvider();

var ClubService = provider.GetRequiredService<IClubService>();
var CountryService = provider.GetRequiredService<ICountryService>();

Console.WriteLine("What to edit?");
Console.WriteLine("\n[C]lubs  |  C[O]untry  |  [Q]uit");

while (true)
{
    var key = Console.ReadKey(intercept: true).Key;
    switch (key)
    {
        case ConsoleKey.C:
            EditClub();
            break;
        case ConsoleKey.O:
            EditCountry();
            break;
        case ConsoleKey.Q:
            return;
    }
}

void EditCountry()
{
    Console.Clear();
    Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit");

    while (true)
    {
        var key = Console.ReadKey(intercept: true).Key;
        switch (key)
        {
            case ConsoleKey.A:
                Console.Clear();
                Console.WriteLine("Adding a new country...");
                Console.WriteLine("Name: ");
                string name = Console.ReadLine();
                CountryService.Add(new Country
                {
                    Id = Guid.NewGuid(),
                    Name = name
                });
                // Add country logic here
                return;
            case ConsoleKey.D:
                Console.Clear();
                Console.WriteLine("Deleting a country...");
                // Delete country logic here
                return;
            case ConsoleKey.E:
                Console.Clear();
                Console.WriteLine("Editing a country...");
                // Edit country logic here
                return;
        }
    }
}

void EditClub()
{
    Console.Clear();
    Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit");
    while (true)
    {
        var key = Console.ReadKey(intercept: true).Key;
        switch (key)
        {
            case ConsoleKey.A:
                Console.Clear();
                Console.WriteLine("Adding a new club...");
                Console.WriteLine("Name: ");
                string name = Console.ReadLine();
                Console.WriteLine("Strength: ");
                int strength = int.Parse(Console.ReadLine() ?? "0");
                ClubService.Add(new Club
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Strength = strength,
                    CountryId = Guid.NewGuid() // Placeholder, should be set appropriately
                });

                return;
            case ConsoleKey.D:
                Console.Clear();
                Console.WriteLine("Deleting a club...");
                // Delete club logic here
                return;
            case ConsoleKey.E:
                Console.Clear();
                Console.WriteLine("Editing a club...");
                // Edit club logic here
                return;
        }
    }
}