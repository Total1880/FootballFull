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

// 1. Kies savegame / DataRoot
Console.WriteLine("Welke save wil je laden? (bv. Default, Save1, Save2)");
var saveName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(saveName))
    saveName = "Default";

var isNew = Configuration.Initialize(saveName: saveName);

Console.WriteLine($"DataRoot: {Configuration.DataRoot}");

// Services
services.AddSingleton<IClubService, ClubService>();
services.AddSingleton<ISeasonService, SeasonService>();
services.AddSingleton<IFixtureService, FixtureService>();
services.AddSingleton<ICompetitionService, CompetitionService>();
services.AddSingleton<IClubPerCompetitionService, ClubPerCompetitionService>();
services.AddSingleton<ICountryService, CountryService>();
services.AddSingleton<IGameService, GameService>();
services.AddSingleton<ITrainerService, TrainerService>();
services.AddSingleton<IClubInternationalRankingService, ClubInternationalRankingService>();
services.AddSingleton<ISaveDataService, SaveDataService>();

// Repositories (V2-varianten)
services.AddSingleton<IRepository<Club>>(
    _ => new ClubRepositoryV2(Path.Combine(Configuration.DataRoot, "Clubs.json")));

services.AddSingleton<IRepository<Competition>>(
    _ => new CompetitionRepositoryV2(Path.Combine(Configuration.DataRoot, "Competitions.json")));

services.AddSingleton<IRepository<Country>>(
    _ => new CountryRepositoryV2(Path.Combine(Configuration.DataRoot, "Countries.json")));

services.AddSingleton<IClubPerCompetitionRepository>(
    _ => new ClubPerCompetitionRepositoryV2(Path.Combine(Configuration.DataRoot, "ClubPerCompetition.json")));

services.AddSingleton<IRepository<Trainer>>(
    _ => new TrainerRepository(Path.Combine(Configuration.DataRoot, "Trainers.json")));

services.AddSingleton<IRepository<ClubInternationalRanking>>(
    _ => new ClubInternationalRankingRepository(Path.Combine(Configuration.DataRoot, "ClubInternationalRanking.json")));

services.AddSingleton<ISaveDataRepository>(
    _ => new SaveDataRepository(Path.Combine(Configuration.DataRoot, "SaveData.json")));

services.AddSingleton<INameRepository>(
    _ => new NameRepository());

var provider = services.BuildServiceProvider();

// Alleen nog de game starten
var gameService = provider.GetRequiredService<IGameService>();
gameService.Run(isNew);
