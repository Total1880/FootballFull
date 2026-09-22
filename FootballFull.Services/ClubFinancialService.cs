using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class ClubFinancialService : IClubFinancialService
    {
        public decimal CalculateClubRevenue(
            Club club,
            Competition competition)
        {
            var baseRevenue = Configuration.BasicClubSeasonRevenue;
            var competitionRevenue = competition.Strength * Configuration.CompetitionRevenueMultiplier;
            var clubRevenue = club.Strength * Configuration.ClubRevenueMultiplier;

            return baseRevenue
                   + competitionRevenue
                   + clubRevenue;
        }

        public decimal CalculateClubExpenses(Club club)
        {
            var baseCosts = Configuration.BasicClubSeasonExpenses;
            var strengthCosts = club.Strength * Configuration.ClubExpenseMultiplier;

            return baseCosts + strengthCosts;
        }

        public ClubFinancialResult CalculateSeasonResult(
            Club club,
            Competition competition)
        {
            var revenue = CalculateClubRevenue(
                club,
                competition);

            var expenses = CalculateClubExpenses(
                club);

            return new ClubFinancialResult
            {
                Revenue = revenue,
                Expenses = expenses
            };
        }
    }
}
