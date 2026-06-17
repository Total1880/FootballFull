using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        IList<ClubInternationalRanking> ClubInternationalRankings { get; }
        void Initialize(IList<ClubPerCompetition> clubs);
        void InitializeNewSeason(int year);
        void PlayMatchDay(IList<Fixture> fixtures, DateTime matchDay, bool isSuddenDeath = false, Guid? playerClubId = null, bool neutralField = false);
        Guid ChoosePlayerClub();
        IList<Fixture> InitializeInternationalGames(DateTime date, bool loadFromSavedGames = false);
        IList<Fixture> InitializeNationalCups(DateTime date);
        Trainer NewTrainer(Guid clubId, DateTime date);
        void UpdateWeekStats(Guid userClubId, DateTime date);
        IList<NewsMessage> NewsMessages { get; }
        int Year { get; set; }
        Trainer UserTrainer(Guid userClubId);
        void SaveGame();
    }
}