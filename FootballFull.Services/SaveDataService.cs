using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
