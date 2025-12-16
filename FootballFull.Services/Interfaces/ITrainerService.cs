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
    }
}
