using FootballFull.Models;
using FootballFull.Services.Interfaces;
using FootballFull.Repositories.Interfaces;

namespace FootballFull.Services
{
    public class SaveDataService : ISaveDataService
    {
        private readonly ISaveDataRepository _repository;
        public SaveData SaveData { get; set; }
        public SaveDataService(ISaveDataRepository repository)
        {
            _repository = repository;

            SaveData = Load();
        }
        public SaveData Load()
        {
            return _repository.Load();
        }

        public void Save()
        {
            _repository.Save(SaveData);
        }
    }
}
