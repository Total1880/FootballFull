using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IPrizeMoneyService
    {
        IReadOnlyList<ClubPrizeMoney> CalculateCupPrizes(CompetitionPrizeMoneySettings settings, Guid winnerId, Guid runnerUpId, IReadOnlyList<Guid> losingSemiFinalistIds);
        IReadOnlyList<ClubPrizeMoney> CalculateLeaguePrizes(CompetitionPrizeMoneySettings settings, IReadOnlyList<Guid> rankedClubIds);
    }
}
