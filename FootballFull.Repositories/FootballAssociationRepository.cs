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
    public class FootballAssociationRepository : IRepository<FootballAssociation>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public FootballAssociationRepository(string path = "data/FootballAssociation.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(FootballAssociation item)
        {
            var items = Load();

            items.Add(item);
            Save(items);
        }

        public IList<FootballAssociation> Create(IList<FootballAssociation> itemList, bool full = false)
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

        public IList<FootballAssociation> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<FootballAssociation>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<FootballAssociation>();
        }

        public void Update(FootballAssociation updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));

            var footballAssociations = Load();

            var index = footballAssociations
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updateItem.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"Football Association with ID {updateItem.Id} not found.");

            // Volledig vervangen door nieuwe versie
            footballAssociations[index.Value] = updateItem;
            Save(footballAssociations);
        }

        private void Save(IList<FootballAssociation> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
