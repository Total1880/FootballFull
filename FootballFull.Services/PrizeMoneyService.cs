using FootballFull.Models;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{

    public class PrizeMoneyService : IPrizeMoneyService
    {
        public IReadOnlyList<ClubPrizeMoney> CalculateLeaguePrizes(
            CompetitionPrizeMoneySettings settings,
            IReadOnlyList<Guid> rankedClubIds)
        {
            ValidateBudget(settings.TotalBudget);

            var numberOfClubs = rankedClubIds.Count;

            var weights = Enumerable.Range(0, numberOfClubs)
                .Select(index =>
                {
                    var rankWeight = numberOfClubs - index;

                    return settings.Distribution switch
                    {
                        PrizeMoneyDistribution.Equal => 1m,
                        PrizeMoneyDistribution.Balanced => (decimal)rankWeight,
                        PrizeMoneyDistribution.TopHeavy =>
                            (decimal)rankWeight * rankWeight,

                        _ => throw new ArgumentException(
                            "Ongeldige verdeelsleutel voor een league.")
                    };
                })
                .ToList();

            return Distribute(
                settings.TotalBudget,
                rankedClubIds,
                weights);
        }

        public IReadOnlyList<ClubPrizeMoney> CalculateCupPrizes(
            CompetitionPrizeMoneySettings settings,
            Guid winnerId,
            Guid runnerUpId,
            IReadOnlyList<Guid> losingSemiFinalistIds)
        {
            ValidateBudget(settings.TotalBudget);

            switch (settings.Distribution)
            {
                case PrizeMoneyDistribution.WinnerOnly:
                    return Distribute(
                        settings.TotalBudget,
                        new[] { winnerId },
                        new[] { 1m });

                case PrizeMoneyDistribution.Finalists:
                    return Distribute(
                        settings.TotalBudget,
                        new[] { winnerId, runnerUpId },
                        new[] { 70m, 30m });

                case PrizeMoneyDistribution.SemiFinalists:
                    if (losingSemiFinalistIds.Count != 2)
                    {
                        throw new ArgumentException(
                            "Deze verdeelsleutel vereist twee verliezende halvefinalisten.");
                    }

                    return Distribute(
                        settings.TotalBudget,
                        new[]
                        {
                        winnerId,
                        runnerUpId,
                        losingSemiFinalistIds[0],
                        losingSemiFinalistIds[1]
                        },
                        new[] { 50m, 25m, 12.5m, 12.5m });

                default:
                    throw new ArgumentException(
                        "Ongeldige verdeelsleutel voor een beker.");
            }
        }

        private static IReadOnlyList<ClubPrizeMoney> Distribute(
            decimal budget,
            IReadOnlyList<Guid> clubIds,
            IReadOnlyList<decimal> weights)
        {
            if (clubIds.Count != weights.Count)
                throw new ArgumentException("Iedere club moet een gewicht hebben.");

            if (clubIds.Distinct().Count() != clubIds.Count)
                throw new ArgumentException("Een club mag maar één prijs ontvangen.");

            if (clubIds.Count == 0 || budget == 0)
                return Array.Empty<ClubPrizeMoney>();

            var totalWeight = weights.Sum();

            // Werk in volledige centen.
            var budgetInCents = decimal.ToInt64(budget * 100m);

            var shares = weights
                .Select(weight => budgetInCents * weight / totalWeight)
                .ToList();

            var allocatedCents = shares
                .Select(share => decimal.ToInt64(decimal.Floor(share)))
                .ToArray();

            var remainingCents = budgetInCents - allocatedCents.Sum();

            // Geef resterende centen aan de grootste afrondingsrest.
            // Bij een gelijke rest krijgt de hoger gerangschikte club voorrang.
            var remainderOrder = Enumerable.Range(0, clubIds.Count)
                .OrderByDescending(index => shares[index] - allocatedCents[index])
                .ThenBy(index => index)
                .ToList();

            for (var i = 0; i < remainingCents; i++)
            {
                allocatedCents[remainderOrder[i]]++;
            }

            return clubIds
                .Select((clubId, index) => new ClubPrizeMoney
                {
                    ClubId = clubId,
                    Amount = allocatedCents[index] / 100m
                })
                .ToList();
        }

        private static void ValidateBudget(decimal budget)
        {
            if (budget < 0 || decimal.Round(budget, 2) != budget)
            {
                throw new ArgumentException(
                    "Het budget moet positief of nul zijn en maximaal twee decimalen bevatten.");
            }
        }
    }
}
