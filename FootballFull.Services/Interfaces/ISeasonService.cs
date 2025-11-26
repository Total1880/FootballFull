using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        void Initialize(IList<ClubPerCompetition> clubs);
        void InitializeNewSeason();
        void PlayMatchDay(IList<Fixture> fixtures, int matchDay,bool isSuddenDeath = false, Guid? playerClubId = null);
        Guid ChoosePlayerClub();
        IList<Fixture> InitializeInternationalGames();
        IList<Fixture> InitializeNationalCups();

        void NewTrainer(Guid clubId);

    }
}