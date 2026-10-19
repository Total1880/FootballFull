using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonService
    {
        IList<ClubLeagueCompetition> ClubLeagueCompetitions { get; }
        ClubInternationalRanking ClubInternationalRankings { get; }
        void Initialize(IList<ClubPerCompetition> clubs);
        IList<ClubPerCompetition> InitializeNewSeason(int year, bool isNew = false);
        bool PlayMatchDay(IList<Fixture> fixtures, DateTime matchDay, bool isSuddenDeath = false, Guid? playerClubId = null, bool neutralField = false);
        Guid ChoosePlayerClub();
        Guid ChoosePlayerCompetition();
        IList<Fixture> InitializeInternationalGames(int year, bool loadFromSavedGames = false);
        IList<Fixture> InitializeNationalCups(DateTime date);
        Trainer NewTrainer(Guid clubId, DateTime date);
        void UpdateWeekStats(Guid userClubId, DateTime date);
        IList<NewsMessage> NewsMessages { get; }
        int Year { get; set; }
        Trainer UserTrainer(Guid userClubId);
        void SaveGame();
        IList<ClubLeagueCompetition> GetRanking(Guid competitionId);
    }
}