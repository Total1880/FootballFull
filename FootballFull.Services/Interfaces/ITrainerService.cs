using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ITrainerService
    {
        IList<Trainer> Load();
        Trainer? GetByClubId(Guid clubId);
        bool CreateTrainers(IList<Trainer> trainers);
        Trainer CreateRandomTrainer(Guid clubId);
        void SaveAll(IList<Trainer> trainers);
        Trainer NewTrainer(Guid clubId, IList<Club> clubs, IList<ClubLeagueCompetition> clubLeagueCompetitions, int year, int week = 0);
        void ClubsFireTrainer(Guid userClubId, int week, IList<Club> clubs, int year, IList<ClubLeagueCompetition> clubLeagueCompetitions);
        IList<NewsMessage> NewsMessages { get; }
    }
}
