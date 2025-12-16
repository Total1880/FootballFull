using FootballFull.Models;
using FootballFull.Services.Interfaces;
using FootballFull.Repositories.Interfaces;

namespace FootballFull.Services
{
    public class SaveDataService : ISaveDataService
    {
        private readonly ISaveDataRepository _repository;
        public SaveDataService(ISaveDataRepository repository)
        {
            _repository = repository;
        }
        public SaveData Load()
        {
            return _repository.Load();
        }

        public void Save(SaveData saveData)
        {
            _repository.Save(saveData);
        }
    }
}
