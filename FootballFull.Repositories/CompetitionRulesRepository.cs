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
    public class CompetitionRulesRepository : IRepository<CompetitionRules>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public CompetitionRulesRepository(string path = "data/CompetitionRules.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(CompetitionRules item)
        {
            var compRules = Load();

            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            compRules.Add(item);
            Save(compRules);
        }

        public IList<CompetitionRules> Create(IList<CompetitionRules> itemList, bool full = false)
        {
            if (full)
            {
                foreach (var compRules in itemList)
                {
                    if (compRules.Id == Guid.Empty)
                        compRules.Id = Guid.NewGuid();
                }

                Save(itemList);
                return itemList;
            }

            var list = Load();

            foreach (var compRules in itemList)
            {
                if (compRules.Id == Guid.Empty)
                    compRules.Id = Guid.NewGuid();

                list.Add(compRules);
            }

            Save(list);
            return itemList;
        }

        public void Delete(Guid id)
        {
            var list = Load();

            var comp = list.FirstOrDefault(c => c.Id == id);
            if (comp == null)
                return;

            list.Remove(comp);
            Save(list);
        }

        public IList<CompetitionRules> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<CompetitionRules>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<CompetitionRules>();
        }

        public void Update(CompetitionRules updateItem)
        {
            var list = Load();

            var index = list
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updateItem.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"Competition Rules with ID {updateItem.Id} not found.");

            list[index.Value] = updateItem;

            Save(list);
        }

        private void Save(IList<CompetitionRules> compRules)
        {
            var json = JsonSerializer.Serialize(compRules, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
