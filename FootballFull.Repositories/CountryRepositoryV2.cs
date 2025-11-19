using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class CountryRepositoryV2 : IRepository<Country>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public CountryRepositoryV2(string path = "data/Countries.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(Country item)
        {
            var list = Load();

            // Als Id niet gevuld is, automatisch aanmaken
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            list.Add(item);
            Save(list);
        }

        public IList<Country> Create(IList<Country> itemList)
        {
            if (itemList == null)
                throw new ArgumentNullException(nameof(itemList));

            var list = Load();

            foreach (var item in itemList)
            {
                if (item.Id == Guid.Empty)
                    item.Id = Guid.NewGuid();

                list.Add(item);
            }

            Save(list);
            return itemList;
        }

        public void Delete(Guid id)
        {
            var list = Load();
            var toRemove = list.FirstOrDefault(c => c.Id == id);
            if (toRemove == null)
                return; // of throw, afhankelijk van wat je wilt

            list.Remove(toRemove);
            Save(list);
        }

        public void Update(Country updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));

            var list = Load();
            var existingIndex = list
                .Select((c, index) => new { c, index })
                .FirstOrDefault(x => x.c.Id == updateItem.Id)?.index;

            if (existingIndex == null)
                throw new InvalidOperationException($"Country with Id {updateItem.Id} not found.");

            // Volledig vervangen door de nieuwe versie
            list[existingIndex.Value] = updateItem;
            Save(list);
        }

        public IList<Country> Load()
        {
            if (!File.Exists(_path))
                return new List<Country>();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Country>>(json, _options) ?? new List<Country>();
        }

        private void Save(IList<Country> countries)
        {
            var json = JsonSerializer.Serialize(countries, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }
    }
}
