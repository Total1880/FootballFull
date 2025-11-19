using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class CompetitionRepositoryV2 : IRepository<Competition>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public CompetitionRepositoryV2(string path = "data/Competitions.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(Competition competition)
        {
            var comps = Load();

            if (competition.Id == Guid.Empty)
                competition.Id = Guid.NewGuid();

            comps.Add(competition);
            Save(comps);
        }

        public IList<Competition> Create(IList<Competition> itemList)
        {
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

        public void Update(Competition updated)
        {
            var list = Load();

            var index = list
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updated.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"Competition with ID {updated.Id} not found.");

            list[index.Value] = updated;

            Save(list);
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

        public IList<Competition> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<Competition>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<Competition>();
        }

        private void Save(IList<Competition> comps)
        {
            var json = JsonSerializer.Serialize(comps, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
