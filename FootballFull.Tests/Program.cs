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
ClubDevelopment.Apply(club, 0);
Equal(5, club.Strength, "Small profits accumulate before growth");
Equal(20_000m, club.DevelopmentBudget, "Half the profit is earmarked");
Equal(100_000m, club.Balance, "Earmarking does not spend cash");
ClubDevelopment.Apply(club, 0);
Equal(5, club.Strength, "Two allocations remain below the investment cost");
ClubDevelopment.Apply(club, 0);
Equal(6, club.Strength, "Accumulated profit enables growth");
Equal(55_000m, club.Balance, "Growth spends the actual investment cost");
Equal(15_000m, club.DevelopmentBudget, "Unused earmarked cash remains");
Equal(45_000m, club.LastSeasonFinancialResult!.DevelopmentInvestment, "Investment is reported");

club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, 1);
Equal(6, club.Strength, "Sporting and financial growth cannot stack beyond one point");
club = Create(balance: -1m, budget: 90_000m);
ClubDevelopment.Apply(club, 1);
Equal(5, club.Strength, "Debt blocks growth");
Equal(0m, club.DevelopmentBudget, "Debt clears unavailable reserves");
club = Create(revenue: 50_000m, expenses: 60_000m);
ClubDevelopment.Apply(club, 1);
Equal(4, club.Strength, "Significant loss outweighs sporting success");
club = Create(balance: -100_001m);
ClubDevelopment.Apply(club, 0);
Equal(4, club.Strength, "Critical debt causes decline");
club = Create(revenue: 59_000m, expenses: 60_000m, budget: 90_000m);
ClubDevelopment.Apply(club, 1);
Equal(5, club.Strength, "A small loss stabilizes but prevents investment growth");
club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, -1);
Equal(4, club.Strength, "Sporting decline still matters");
Equal(0m, club.LastSeasonFinancialResult!.DevelopmentInvestment, "No investment on decline");
club = Create(balance: 10_000m, budget: 90_000m);
ClubDevelopment.Apply(club, 0);
Equal(10_000m, club.DevelopmentBudget, "Earmarked budget cannot exceed cash");
club = Create(strength: 20, balance: 1_000_000m, budget: 900_000m);
ClubDevelopment.Apply(club, 1);
Equal(20, club.Strength, "Upper strength limit");
Equal(1_000_000m, club.Balance, "No wasted investment at upper limit");
club = Create(strength: 1, revenue: 0);
ClubDevelopment.Apply(club, -1);
Equal(1, club.Strength, "Lower strength limit");
var legacy = JsonSerializer.Deserialize<Club>("{\"Name\":\"Legacy\",\"Strength\":5}")!;
ClubDevelopment.Apply(legacy, 1);
Equal(5, legacy.Strength, "Legacy save without a result loads safely");
Equal(0m, legacy.DevelopmentBudget, "Legacy budget defaults to zero");
club = Create(budget: 90_000m);
ClubDevelopment.Apply(club, 0);
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
        "Update" => null,
        _ => throw new NotSupportedException(name)
    });
    var membershipService = Stub<IClubPerCompetitionService>.Create((name, args) => name switch
    {
        "GetAllClubPerCompetitionForCountry" => memberships,
        "GetClubsForCompetition" => clubService.GetClubs(),
        _ => throw new NotSupportedException(name)
    });
    var finance = new ClubFinancialService();
    new EndOfSeasonService(null!, clubService, null!, competitionService, null!,
        membershipService, finance).ProcessClubFinances(countryId);
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
    Equal(20, cup.Strength, "Domestic cups are unaffected");
}
finally { if (File.Exists(path)) File.Delete(path); }
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
