// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
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
services.AddSingleton<IGameService, GameService>();

// Repositories (V2-varianten)
services.AddSingleton<IRepository<Club>>(
    _ => new ClubRepositoryV2(Path.Combine(Configuration.DataRoot, "Clubs.json")));

services.AddSingleton<IRepository<Competition>>(
    _ => new CompetitionRepositoryV2(Path.Combine(Configuration.DataRoot, "Competitions.json")));

services.AddSingleton<IRepository<Country>>(
    _ => new CountryRepositoryV2(Path.Combine(Configuration.DataRoot, "Countries.json")));

services.AddSingleton<IClubPerCompetitionRepository>(
    _ => new ClubPerCompetitionRepositoryV2(Path.Combine(Configuration.DataRoot, "ClubPerCompetition.json")));

var provider = services.BuildServiceProvider();

// Alleen nog de game starten
var gameService = provider.GetRequiredService<IGameService>();
gameService.Run();
