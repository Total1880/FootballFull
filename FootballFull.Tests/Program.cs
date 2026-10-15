using System.Reflection;
using System.Text.Json;
using FootballFull.Models;
using FootballFull.Repositories;
using FootballFull.Services;
using FootballFull.Services.Interfaces;

// Dependency-free regression checks: dotnet run --project FootballFull.Tests
var checks = 0;
void Equal<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"{name}: expected {expected}, actual {actual}");
    checks++;
}
Club Create(int strength = 5, decimal balance = 100_000m, decimal revenue = 100_000m,
    decimal expenses = 60_000m, decimal budget = 0) => new()
{
    Id = Guid.NewGuid(), Name = "Test Club", Strength = strength, Balance = balance,
    DevelopmentBudget = budget,
    LastSeasonFinancialResult = new() { Revenue = revenue, Expenses = expenses }
};

var club = Create();
ClubDevelopment.Apply(club, 0, null);
Equal(5, club.Strength, "Small profits accumulate before growth");
Equal(20_000m, club.DevelopmentBudget, "Half the profit is earmarked");
Equal(100_000m, club.Balance, "Earmarking does not spend cash");
ClubDevelopment.Apply(club, 0, null);
Equal(5, club.Strength, "Two allocations remain below the investment cost");
ClubDevelopment.Apply(club, 0, null);
Equal(6, club.Strength, "Accumulated profit enables growth");
Equal(55_000m, club.Balance, "Growth spends the actual investment cost");
Equal(15_000m, club.DevelopmentBudget, "Unused earmarked cash remains");
Equal(45_000m, club.LastSeasonFinancialResult!.DevelopmentInvestment, "Investment is reported");

club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, 1, null);
Equal(6, club.Strength, "Sporting and financial growth cannot stack beyond one point");
club = Create(balance: -1m, budget: 90_000m);
ClubDevelopment.Apply(club, 1, null);
Equal(5, club.Strength, "Debt blocks growth");
Equal(0m, club.DevelopmentBudget, "Debt clears unavailable reserves");
club = Create(revenue: 50_000m, expenses: 60_000m);
ClubDevelopment.Apply(club, 1, null);
Equal(4, club.Strength, "Significant loss outweighs sporting success");
club = Create(balance: -100_001m);
ClubDevelopment.Apply(club, 0, null);
Equal(4, club.Strength, "Critical debt causes decline");
club = Create(revenue: 59_000m, expenses: 60_000m, budget: 90_000m);
ClubDevelopment.Apply(club, 1, null);
Equal(5, club.Strength, "A small loss stabilizes but prevents investment growth");
club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, -1, null);
Equal(4, club.Strength, "Sporting decline still matters");
Equal(0m, club.LastSeasonFinancialResult!.DevelopmentInvestment, "No investment on decline");
club = Create(balance: 10_000m, budget: 90_000m);
ClubDevelopment.Apply(club, 0, null);
Equal(10_000m, club.DevelopmentBudget, "Earmarked budget cannot exceed cash");
club = Create(strength: 20, balance: 1_000_000m, budget: 900_000m);
ClubDevelopment.Apply(club, 1, null);
Equal(20, club.Strength, "Upper strength limit");
Equal(1_000_000m, club.Balance, "No wasted investment at upper limit");
club = Create(strength: 1, revenue: 0);
ClubDevelopment.Apply(club, -1, null);
Equal(1, club.Strength, "Lower strength limit");
var legacy = JsonSerializer.Deserialize<Club>("{\"Name\":\"Legacy\",\"Strength\":5}")!;
ClubDevelopment.Apply(legacy, 1, null);
Equal(5, legacy.Strength, "Legacy save without a result loads safely");
Equal(0m, legacy.DevelopmentBudget, "Legacy budget defaults to zero");
club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, 0, null);
var roundTrip = JsonSerializer.Deserialize<Club>(JsonSerializer.Serialize(club))!;
Equal(club.DevelopmentBudget, roundTrip.DevelopmentBudget, "Budget JSON round trip");
Equal(club.LastSeasonFinancialResult!.DevelopmentInvestment,
    roundTrip.LastSeasonFinancialResult!.DevelopmentInvestment, "Result JSON round trip");

// Exercise the real season-finance and strength services against a temporary JSON repository.
var path = Path.Combine(Path.GetTempPath(), $"football-tests-{Guid.NewGuid()}.json");
try
{
    var countryId = Guid.NewGuid();
    var league = new Competition { Id = Guid.NewGuid(), Name = "League", CountryId = countryId,
        Strength = 5, Tier = 1, Type = Competition.CompetitionType.League };
    var secondLeague = new Competition { Id = Guid.NewGuid(), Name = "Split", CountryId = countryId,
        Strength = 1, Tier = 2, Type = Competition.CompetitionType.League };
    var cup = new Competition { Id = Guid.NewGuid(), Name = "Cup", CountryId = countryId,
        Strength = 20, Type = Competition.CompetitionType.Cup };
    IList<Competition> competitions = new List<Competition> { league, secondLeague, cup };
    club = Create(strength: 1, balance: 0);
    club.CountryId = countryId;
    var clubService = new ClubService(new ClubRepositoryV2(path));
    clubService.Add(club);
    IList<ClubPerCompetition> memberships = competitions.Select(c => new ClubPerCompetition
        { ClubId = club.Id, CompetitionId = c.Id }).ToList();
    var competitionService = Stub<ICompetitionService>.Create((name, args) => name switch
    {
        "GetCompetitions" or "GetCompetitionsForCountry" => competitions,
        "GetCompetitionById" => competitions.Single(c => c.Id == (Guid)args[0]!),
        "Update" => ReplaceCompetition((Competition)args[0]!),
        _ => throw new NotSupportedException(name)
    });
    object? ReplaceCompetition(Competition updated)
    {
        // Reproduce catalogue updates that invalidate a List enumerator.
        competitions[competitions.ToList().FindIndex(c => c.Id == updated.Id)] = updated;
        return null;
    }
    var membershipService = Stub<IClubPerCompetitionService>.Create((name, args) => name switch
    {
        "GetAllClubPerCompetitionForCountry" => memberships,
        "GetClubsForCompetition" => clubService.GetClubs(),
        _ => throw new NotSupportedException(name)
    });
    var finance = new ClubFinancialService();
    new EndOfSeasonService(Stub<ISeasonService>.Create((name, args) => name == "get_Year" ? 2026 : throw new NotSupportedException(name)), clubService, null!, competitionService, null!,
        membershipService, finance, null!, new PrizeMoneyService(), null!).ProcessClubFinances(countryId);
    club = clubService.GetClubById(club.Id)!;
    Equal(53_000m, club.Balance, "Only one league season is booked, cups and splits excluded");
    Equal(105_000m, club.LastSeasonFinancialResult!.Revenue, "Uses primary league revenue");
    var rankingService = Stub<IClubLeagueCompetitionService>.Create((name, args) => name switch
    {
        "GetClubLeagueCompetitionsByCompetitionId" => new List<ClubLeagueCompetition>
            { new() { ClubId = club.Id, CompetitionId = (Guid)args[0]!, MatchesPlayed = 10 } },
        "GetOrderedRanking" => args[0],
        _ => throw new NotSupportedException(name)
    });
    var strength = new StrengthService(clubService, competitionService, rankingService, membershipService);
    strength.RecalculateClubStrengths();
    club = clubService.GetClubById(club.Id)!;
    Equal(26_500m, club.DevelopmentBudget, "Duplicate league memberships allocate profit only once");
    strength.RecalculateCompetitionStrengths(2026);
    Equal(club.Strength, league.Strength, "League strength follows member clubs");
    Equal(club.Strength, secondLeague.Strength, "Every league is processed despite catalogue updates");
    Equal(20, cup.Strength, "Domestic cups are unaffected");
}
finally { if (File.Exists(path)) File.Delete(path); }
// Prize money: exercise reservation, league/cup payouts and reload through JSON repositories.
var prizePath = Path.Combine(Path.GetTempPath(), "footballfull-prizes-" + Guid.NewGuid());
Directory.CreateDirectory(prizePath);
try
{
    var countryId = Guid.NewGuid();
    var clubsRepository = new ClubRepositoryV2(Path.Combine(prizePath, "clubs.json"));
    var competitionsRepository = new CompetitionRepositoryV2(Path.Combine(prizePath, "competitions.json"));
    var prizeClubs = Enumerable.Range(0, 4).Select(_ => Create(balance: 0)).ToList();
    clubsRepository.Create(prizeClubs, true);
    var league = new Competition { Id = Guid.NewGuid(), Name = "League", CountryId = countryId };
    var cup = new Competition { Id = Guid.NewGuid(), Name = "Cup", CountryId = countryId,
        Type = Competition.CompetitionType.Cup };
    competitionsRepository.Create(new List<Competition> { league, cup }, true);
    var competitionCatalogue = competitionsRepository.Load();
    var competitions = Stub<ICompetitionService>.Create((name, args) => name switch
    {
        "GetCompetitions" or "GetCompetitionsForCountry" => competitionCatalogue,
        "Update" => SaveCompetition((Competition)args[0]!),
        _ => throw new NotSupportedException(name)
    });
    object? SaveCompetition(Competition value)
    {
        competitionsRepository.Update(value);
        competitionCatalogue[competitionCatalogue.ToList().FindIndex(c => c.Id == value.Id)] = value;
        return null;
    }
    var season = Stub<ISeasonService>.Create((name, args) => name switch
    {
        "get_Year" => 2026,
        "GetRanking" => prizeClubs.Select(c => new ClubLeagueCompetition { ClubId = c.Id }).ToList(),
        _ => throw new NotSupportedException(name)
    });
    var memberships = Stub<IClubPerCompetitionService>.Create((name, args) => name switch
    {
        "GetAllClubPerCompetitionForCountry" => prizeClubs.Select(c => new ClubPerCompetition
            { ClubId = c.Id, CompetitionId = league.Id }).ToList(),
        _ => throw new NotSupportedException(name)
    });
    var association = new FootballAssociation { CountryId = countryId, Balance = 1_000m };
    var associations = Stub<IFootballAssociationsService>.Create((name, args) =>
        name == "Update" ? null : throw new NotSupportedException(name));
    var calculator = new PrizeMoneyService();
    var closing = new EndOfSeasonService(season, new ClubService(clubsRepository), null!, competitions,
        null!, memberships, new ClubFinancialService(), null!, calculator, associations);
    var settings = new CompetitionPrizeMoneySettings { TotalBudget = 100m,
        Distribution = PrizeMoneyDistribution.Balanced };
    closing.ReservePrizeMoney(league, settings, association, 2026);
    Equal(900m, association.Balance, "League pot is reserved once");
    settings.TotalBudget = 999m;
    Equal(100m, league.PrizeMoneySeasons[0].Settings.TotalBudget, "Season settings are a snapshot");
    try { closing.ReservePrizeMoney(league, settings, association, 2026); throw new Exception("Duplicate reservation accepted"); }
    catch (InvalidOperationException) { checks++; }
    try { closing.ReservePrizeMoney(cup, new() { TotalBudget = 901m,
        Distribution = PrizeMoneyDistribution.WinnerOnly }, association, 2026); throw new Exception("Unaffordable pot accepted"); }
    catch (InvalidOperationException) { checks++; }
    Equal(900m, association.Balance, "Rejected reservation leaves balance unchanged");
    closing.ReservePrizeMoney(cup, new() { TotalBudget = 200m,
        Distribution = PrizeMoneyDistribution.SemiFinalists }, association, 2026);
    var fixtures = new List<Fixture>
    {
        new() { CompetitionId = cup.Id, RoundNo = 1, HomeTeamId = prizeClubs[0].Id,
            AwayTeamId = prizeClubs[2].Id, HomeScore = 1 },
        new() { CompetitionId = cup.Id, RoundNo = 1, HomeTeamId = prizeClubs[1].Id,
            AwayTeamId = prizeClubs[3].Id, HomeScore = 1 },
        new() { CompetitionId = cup.Id, RoundNo = 2, HomeTeamId = prizeClubs[0].Id,
            AwayTeamId = prizeClubs[1].Id, HomeScore = 1 }
    };
    closing.ProcessPrizeMoney(fixtures, null);
    Equal(140m, clubsRepository.Load()[0].Balance, "Champion receives league and cup prizes");
    Equal(80m, clubsRepository.Load()[1].Balance, "Runner-up receives both prizes");
    Equal(300m, clubsRepository.Load().Sum(c => c.Balance), "All reserved money is distributed");
    Equal(700m, association.Balance, "Payout does not charge the association twice");
    competitionCatalogue = competitionsRepository.Load();
    closing.ProcessPrizeMoney(fixtures, null);
    Equal(300m, clubsRepository.Load().Sum(c => c.Balance), "Reload and repeat do not pay twice");
    // Simulate retry after club balances were written but completion was not recorded.
    foreach (var c in competitionCatalogue)
    {
        c.PrizeMoneySeasons[0].IsPaid = false;
        c.PrizeMoneySeasons[0].Payments.Clear();
    }
    closing.ProcessPrizeMoney(fixtures, null);
    Equal(300m, clubsRepository.Load().Sum(c => c.Balance), "Club payment records prevent duplicate credits on retry");
    closing.ProcessClubFinances(countryId);
    Equal(140m, clubsRepository.Load()[0].LastSeasonFinancialResult!.PrizeMoneyReceived,
        "Prize money is included in the financial result");
    Equal(clubsRepository.Load()[0].LastSeasonFinancialResult!.NetResult, clubsRepository.Load()[0].Balance,
        "Financial close does not credit prizes twice");
    foreach (var distribution in new[] { PrizeMoneyDistribution.Equal, PrizeMoneyDistribution.Balanced,
        PrizeMoneyDistribution.TopHeavy })
    {
        var prizes = calculator.CalculateLeaguePrizes(new() { TotalBudget = 0.01m,
            Distribution = distribution }, prizeClubs.Select(c => c.Id).ToList());
        Equal(0.01m, prizes.Sum(p => p.Amount), "Rounding preserves the full pot");
    }
    Equal(0, JsonSerializer.Deserialize<Competition>("{}")!.PrizeMoneySeasons.Count,
        "Older saves default to no reservations");
}
finally { Directory.Delete(prizePath, true); }
Console.WriteLine($"PASS: {checks} regression checks.");

public class Stub<T> : DispatchProxy where T : class
{
    public Func<string, object?[], object?> Handler { get; set; } = null!;
    public static T Create(Func<string, object?[], object?> handler)
    {
        var proxy = DispatchProxy.Create<T, Stub<T>>();
        ((Stub<T>)(object)proxy).Handler = handler;
        return proxy;
    }
    protected override object? Invoke(MethodInfo? method, object?[]? args) =>
        Handler(method!.Name, args ?? Array.Empty<object?>());
}
