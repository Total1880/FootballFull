using FootballFull.Models;
using OlavFramework;

namespace FootballFull.Services;

/// <summary>Applies one season of development after the club's financial result is booked.</summary>
internal static class ClubDevelopment
{
    public static void Apply(Club club, int sportingDelta)
    {
        var result = club.LastSeasonFinancialResult;
        if (result == null)
            return; // Old saves have no result until their first season closes.

        result.StrengthBefore = club.Strength;
        result.DevelopmentInvestment = 0;
        var availableCash = Math.Max(0m, club.Balance);
        var allocation = Math.Max(0m, result.NetResult) * Configuration.ClubDevelopmentProfitShare;
        club.DevelopmentBudget = Math.Clamp(club.DevelopmentBudget + allocation, 0m, availableCash);

        var investmentCost = Configuration.ClubDevelopmentBaseCost
            + club.Strength * Configuration.ClubDevelopmentStrengthCost;
        var significantLoss = result.NetResult < 0
            && -result.NetResult >= result.Expenses * Configuration.ClubSignificantLossRatio;
        var criticalDebt = club.Balance < -Configuration.ClubCriticalDebtThreshold;
        int delta;
        if (significantLoss || criticalDebt)
        {
            delta = -1;
            result.DevelopmentReason = criticalDebt ? "Zware schulden" : "Aanzienlijk seizoensverlies";
        }
        else if (sportingDelta < 0)
        {
            delta = -1;
            result.DevelopmentReason = "Sportieve terugval";
        }
        else if (result.NetResult > 0 && club.Balance >= 0
            && club.DevelopmentBudget >= investmentCost && investmentCost > 0)
        {
            delta = 1;
            result.DevelopmentReason = "Investering uit opgebouwde winst";
        }
        else
        {
            delta = 0;
            result.DevelopmentReason = sportingDelta > 0
                ? "Sportieve groei wacht op voldoende investeringsbudget"
                : "Budget opbouwen / stabiliseren";
        }

        var newStrength = Math.Clamp(club.Strength + delta,
            Configuration.MinStrength, Configuration.MaxStrength);
        if (newStrength > club.Strength)
        {
            club.Balance -= investmentCost;
            club.DevelopmentBudget -= investmentCost;
            result.DevelopmentInvestment = investmentCost;
        }
        club.Strength = newStrength;
        result.StrengthAfter = newStrength;
        if (delta != 0 && result.StrengthBefore == newStrength)
            result.DevelopmentReason = "Strength-limiet bereikt";
    }
}
