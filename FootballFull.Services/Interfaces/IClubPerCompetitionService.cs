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
        IList<ClubPerCompetition> GetAllClubPerSpecificCompetitions(Guid competitionId);
        IList<ClubPerCompetition> GetAllClubPerCompetitionForCountry(Guid countryId);
        void SaveAll(IList<ClubPerCompetition> clubPerCompetition);
        bool DeleteEverythingForCompetition(Guid competitionId);
        bool DeleteEverythingForClub(Guid clubId);
    }
}
