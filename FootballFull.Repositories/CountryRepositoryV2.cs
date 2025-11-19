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
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(Country updateItem)
        {
            throw new NotImplementedException();
        }

        public IList<Country> Load()
        {
            var countries = new List<Country>
            {
                new Country { Id = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1"), Name = "Belgium" },
            };

            return countries;
        }

        private void Save(IList<Country> clubs)
        {
            var json = JsonSerializer.Serialize(clubs, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }

    }
}
