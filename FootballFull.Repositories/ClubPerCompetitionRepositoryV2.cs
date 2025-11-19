using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class ClubPerCompetitionRepositoryV2 : IRepository<ClubPerCompetition>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public ClubPerCompetitionRepositoryV2(string path = "data/ClubPerCompetition.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(ClubPerCompetition item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var list = Load();

            // dubbele koppelingen vermijden
            if (!list.Any(x => x.ClubId == item.ClubId && x.CompetitionId == item.CompetitionId))
            {
                list.Add(item);
                Save(list);
            }
        }

        public IList<ClubPerCompetition> Create(IList<ClubPerCompetition> itemList)
        {
            if (itemList == null)
                throw new ArgumentNullException(nameof(itemList));

            var list = Load();

            foreach (var item in itemList)
            {
                if (!list.Any(x => x.ClubId == item.ClubId && x.CompetitionId == item.CompetitionId))
                    list.Add(item);
            }

            Save(list);
            return itemList;
        }

        public void Update(ClubPerCompetition item)
        {
            // In de praktijk heb je zelden een “update” op een link;
            // eventueel: eerst delete, dan add. Voor nu: no-op of exception:
            throw new NotSupportedException("Update is not supported for ClubPerCompetition.");
        }

        public void Delete(Guid id)
        {
            // Niet bruikbaar, want er is geen single Guid Id.
            // We gaan dus geen Delete(Guid) gebruiken, maar een eigen methode in de service.
            throw new NotSupportedException("Delete by Id is not supported for ClubPerCompetition.");
        }

        public IList<ClubPerCompetition> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<ClubPerCompetition>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<ClubPerCompetition>();
        }

        private void Save(IList<ClubPerCompetition> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
