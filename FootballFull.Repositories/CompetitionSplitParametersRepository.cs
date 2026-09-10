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
    public class CompetitionSplitParametersRepository : IRepository<CompetitionSplitParameters>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public CompetitionSplitParametersRepository(string path = "data/CompetitionSplitParameters.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
        public void Add(CompetitionSplitParameters item)    
        {
            var items = Load();

            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            items.Add(item);
            Save(items);
        }

        public IList<CompetitionSplitParameters> Create(IList<CompetitionSplitParameters> itemList, bool full = false)
        {
            if (full)
            {
                foreach (var comp in itemList)
                {
                    if (comp.Id == Guid.Empty)
                        comp.Id = Guid.NewGuid();
                }

                Save(itemList);
                return itemList;
            }

            var list = Load();

            foreach (var comp in itemList)
            {
                if (comp.Id == Guid.Empty)
                    comp.Id = Guid.NewGuid();

                list.Add(comp);
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

        public IList<CompetitionSplitParameters> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<CompetitionSplitParameters>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<CompetitionSplitParameters>();
        }

        public void Update(CompetitionSplitParameters updateItem)
        {
            var list = Load();

            var index = list
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updateItem.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"Competition Split Parameter with ID {updateItem.Id} not found.");

            list[index.Value] = updateItem;

            Save(list);
        }

        private void Save(IList<CompetitionSplitParameters> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
