using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        IList<ClubInternationalRanking> ClubInternationalRankings { get; }
        void Initialize(IList<ClubPerCompetition> clubs);
        void InitializeNewSeason(int year);
        void PlayMatchDay(IList<Fixture> fixtures, int matchDay,bool isSuddenDeath = false, Guid? playerClubId = null);
        Guid ChoosePlayerClub();
        IList<Fixture> InitializeInternationalGames();
        IList<Fixture> InitializeNationalCups();
        Trainer NewTrainer(Guid clubId, int matchDay = 0);
        void UpdateWeekStats(Guid userClubId, int matchDay);
        IList<NewsMessage> NewsMessages { get; }
        int Year { get; set; }
        Trainer UserTrainer(Guid userClubId);
        void SaveGame();
    }
}