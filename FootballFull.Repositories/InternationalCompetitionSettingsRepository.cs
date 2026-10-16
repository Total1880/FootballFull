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
    public class InternationalCompetitionSettingsRepository : IRepository<InternationalCompetitionSettings>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public InternationalCompetitionSettingsRepository(string path = "data/InternationalCompetitionSettings.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(InternationalCompetitionSettings item)
        {
            var items = Load();

            items.Add(item);
            Save(items);
        }

        public IList<InternationalCompetitionSettings> Create(IList<InternationalCompetitionSettings> itemList, bool full = false)
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

        public IList<InternationalCompetitionSettings> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<InternationalCompetitionSettings>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<InternationalCompetitionSettings>();
        }

        public void Update(InternationalCompetitionSettings updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));

            var internationalCompetitionSettings = Load();

            var index = internationalCompetitionSettings
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.CompetitionId == updateItem.CompetitionId)?.i;

            if (index == null)
                throw new InvalidOperationException($"International Competition Settings with ID {updateItem.CompetitionId} not found.");

            // Volledig vervangen door nieuwe versie
            internationalCompetitionSettings[index.Value] = updateItem;
            Save(internationalCompetitionSettings);
        }

        private void Save(IList<InternationalCompetitionSettings> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
