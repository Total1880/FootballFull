using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISaveDataService
    {
        void Save(SaveData saveData);
        SaveData Load();
    }
}
