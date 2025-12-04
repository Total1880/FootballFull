using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface IClubPerCompetitionService
    {
        void AddClubToCompetition(Guid clubId, Guid competitionId);
        void RemoveClubFromCompetition(Guid clubId, Guid competitionId);

        IList<Club> GetClubsForCompetition(Guid competitionId);
        IList<Competition> GetCompetitionsForClub(Guid clubId);
        IList<ClubPerCompetition> GetAllClubPerCompetitions();
        void SaveAll(IList<ClubPerCompetition> clubPerCompetition);
    }
}
