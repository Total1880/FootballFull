using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISaveDataService
    {
        void Save();
        public SaveData SaveData { get; set; }
        SaveData Load();
    }
}
