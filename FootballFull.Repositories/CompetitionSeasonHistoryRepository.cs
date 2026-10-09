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
    public class CompetitionSeasonHistoryRepository : IRepository<CompetitionSeasonHistory>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public CompetitionSeasonHistoryRepository(string path = "data/CompetitionSeasonHistory.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(CompetitionSeasonHistory item)
        {
            var items = Load();

            items.Add(item);
            Save(items);
        }

        public IList<CompetitionSeasonHistory> Create(IList<CompetitionSeasonHistory> itemList, bool full = false)
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

        public IList<CompetitionSeasonHistory> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<CompetitionSeasonHistory>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<CompetitionSeasonHistory>();
        }

        public void Update(CompetitionSeasonHistory updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));

            var items = Load();

            var index = items
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updateItem.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"CompetitionSeasonHistory with ID {updateItem.Id} not found.");

            // Volledig vervangen door nieuwe versie
            items[index.Value] = updateItem;
            Save(items);
        }

        private void Save(IList<CompetitionSeasonHistory> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
