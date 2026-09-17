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
    public class SeasonFinancialResultService : ISeasonFinancialResultService
    {
        public SeasonFinancialResult CalculateFinancialResults(int clubsCount, int competitionsCount, FootballAssociation association)
        {
            var seasonFinancialResult = new SeasonFinancialResult();

            seasonFinancialResult.ClubIncome = clubsCount * Configuration.BasicClubRevenue;
            seasonFinancialResult.ReputationIncome = association.Reputation * Configuration.ReputationRevenueMultiplier;
            seasonFinancialResult.BonusIncome = 0; // Placeholder for any bonus income logic

            seasonFinancialResult.ClubCosts = clubsCount * Configuration.BasicClubCost;
            seasonFinancialResult.CompetitionCosts = competitionsCount * Configuration.BasicCompetitionCost;
            seasonFinancialResult.OrganisationCosts = Configuration.BasicOrganisationCost;

            seasonFinancialResult.ReputationChange = +CalculateReputationChange(association);

            association.Balance += seasonFinancialResult.NetResult;
            association.Reputation = association.Reputation >= Configuration.MaxReputation ?
                Configuration.MaxReputation :
                association.Reputation <= 1 ?
                1 : association.Reputation + seasonFinancialResult.ReputationChange;

            return seasonFinancialResult;
        }

        private int CalculateReputationChange(FootballAssociation fa)
        {
            var change = 0;

            if (fa.Balance > 0)
                change += 1;

            if (fa.Balance > Configuration.ReputationBalanceThreshold)
                change += 1;

            return change;
        }
    }
}
