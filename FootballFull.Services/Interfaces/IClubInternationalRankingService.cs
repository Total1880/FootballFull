using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IClubInternationalRankingService
    {
        IList<ClubInternationalRanking> GetAll();
        void SaveAll(IList<ClubInternationalRanking> clubs);
    }
}
