using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class PlayerCompetitionSettingsRepository : IRepository<PlayerCompetitionSettings>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public PlayerCompetitionSettingsRepository(string path = "data/PlayerCompetitionSettings.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(PlayerCompetitionSettings item)
        {
            var items = Load();

            items.Add(item);
            Save(items);
        }

        public IList<PlayerCompetitionSettings> Create(IList<PlayerCompetitionSettings> itemList, bool full = false)
        {
            if (full)
            {
                Save(itemList);
                return itemList;
            }

            var list = Load();

            foreach (var comp in itemList)
            {
                list.Add(comp);
            }

            Save(list);
            return itemList;
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IList<PlayerCompetitionSettings> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<PlayerCompetitionSettings>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<PlayerCompetitionSettings>();
        }

        public void Update(PlayerCompetitionSettings updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));

            Save(new List<PlayerCompetitionSettings> { updateItem });
        }

        private void Save(IList<PlayerCompetitionSettings> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
