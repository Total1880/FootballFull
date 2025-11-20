using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        void Initialize(IList<ClubPerCompetition> clubs);
        void InitializeNewSeason();
        void PlayMatchDay(IList<Fixture> fixtures, int matchDay, Guid? playerClubId = null);
        Guid ChoosePlayerClub();

    }
}