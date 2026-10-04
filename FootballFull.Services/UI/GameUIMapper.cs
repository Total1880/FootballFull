using FootballFull.Models;
using FootballFull.Services.UI.ViewModels;

namespace FootballFull.Services.UI;

internal static class GameUIMapper
{
    public static List<SelectionOptionViewModel> Options(IEnumerable<Competition> items) =>
        items.Select(x => new SelectionOptionViewModel { Id = x.Id, Name = x.Name }).ToList();
    public static List<SelectionOptionViewModel> Options(IEnumerable<Country> items) =>
        items.Select(x => new SelectionOptionViewModel { Id = x.Id, Name = x.Name }).ToList();
    public static List<SelectionOptionViewModel> Options(IEnumerable<Club> items) =>
        items.Select(x => new SelectionOptionViewModel { Id = x.Id, Name = x.Name }).ToList();
    public static List<FixtureViewModel> Fixtures(IEnumerable<Fixture>? items) =>
        (items ?? Array.Empty<Fixture>()).Select(x => new FixtureViewModel
        {
            HomeTeamName = x.HomeTeam?.Name ?? "TBD",
            AwayTeamName = x.AwayTeam?.Name ?? "TBD",
            HomeScore = x.HomeScore, AwayScore = x.AwayScore, MatchDay = x.MatchDay
        }).ToList();
    public static SeasonEventViewModel Event(SeasonEvent x) => new()
    {
        Description = x.Description, BalanceChange = x.BalanceChange, ReputationChange = x.ReputationChange
    };
    public static SeasonFinancialResultViewModel Finances(SeasonFinancialResult x) => new()
    {
        AssociationName = x.FootballAssociation.Name,
        Balance = x.FootballAssociation.Balance,
        Reputation = x.FootballAssociation.Reputation,
        ReputationDescription = x.FootballAssociation.ReputationDescription,
        ClubIncome = x.ClubIncome, ReputationIncome = x.ReputationIncome, BonusIncome = x.BonusIncome,
        ClubCosts = x.ClubCosts, CompetitionCosts = x.CompetitionCosts, OrganisationCosts = x.OrganisationCosts,
        ReputationChange = x.ReputationChange, NetResult = x.NetResult
    };
    public static List<CountryRankingViewModel> Rankings(IEnumerable<CountryCoefficientRanking> items) =>
        items.Select(x => new CountryRankingViewModel
        {
            CountryName = x.Country?.Name ?? x.CountryId.ToString(),
            FiveYearCoefficient = x.FiveYearCoefficient,
            CoefficientPerYear = new Dictionary<int, double>(x.CoefficientPerYear),
            ClubsParticipatingPerYear = new Dictionary<int, int>(x.ClubsParticipatingPerYear),
            RawPointsPerYear = new Dictionary<int, int>(x.RawPointsPerYear)
        }).ToList();
    public static List<NewsMessageViewModel> News(IEnumerable<NewsMessage> items) =>
        items.Select(x => new NewsMessageViewModel { Message = x.Message, Date = x.Date }).ToList();
}
