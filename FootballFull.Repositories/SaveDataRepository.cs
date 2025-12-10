using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class SaveDataRepository : ISaveDataRepository
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;
        public SaveDataRepository(string path)
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
        public SaveData Load()
        {
            if (!File.Exists(_path))
                return new SaveData();

            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<SaveData>(json, _options) ?? new SaveData();
            return data ?? new SaveData();
        }

        public void Save(SaveData saveData)
        {
            var json = JsonSerializer.Serialize(saveData, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }
    }
}
