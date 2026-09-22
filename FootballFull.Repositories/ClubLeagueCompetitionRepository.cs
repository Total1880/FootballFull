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
    public class ClubLeagueCompetitionRepository : IRepository<ClubLeagueCompetition>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public ClubLeagueCompetitionRepository(string path = "data/ClubLeagueCompetition.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(ClubLeagueCompetition item)
        {
            var items = Load();

            items.Add(item);
            Save(items);
        }

        public IList<ClubLeagueCompetition> Create(IList<ClubLeagueCompetition> itemList, bool full = false)
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

        public IList<ClubLeagueCompetition> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<ClubLeagueCompetition>>(json, _options);
                if (list != null)
                    return list;
            }

            return new List<ClubLeagueCompetition>();
        }

        public void Update(ClubLeagueCompetition updateItem)
        {
            var list = Load();

            var index = list
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.ClubId == updateItem.ClubId && x.c.CompetitionId == updateItem.CompetitionId)?.i;

            if (index == null)
                throw new InvalidOperationException($"Club-League-Competition entry with Club ID {updateItem.ClubId} and Competition ID {updateItem.CompetitionId} not found.");

            list[index.Value] = updateItem;

            Save(list);
        }

        private void Save(IList<ClubLeagueCompetition> items)
        {
            var json = JsonSerializer.Serialize(items, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
}
