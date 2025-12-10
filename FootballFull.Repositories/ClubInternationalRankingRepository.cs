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
    public class ClubInternationalRankingRepository : IRepository<ClubInternationalRanking>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public ClubInternationalRankingRepository(string path)
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
        public void Add(ClubInternationalRanking item)
        {
            throw new NotImplementedException();
        }

        public IList<ClubInternationalRanking> Create(IList<ClubInternationalRanking> itemList, bool full = false)
        {
            if (itemList == null)
                throw new ArgumentNullException(nameof(itemList));

            if (!full)
            {
                var clubInternationRanking = Load();

                foreach (var item in itemList)
                {
                    if (item.ClubId == Guid.Empty)
                        throw new ArgumentException("ClubInternationalRanking must have a valid ClubId");

                    clubInternationRanking.Add(item);
                }

                Save(clubInternationRanking);
            }
            else
            {
                // Volledig overschrijven
                foreach (var item in itemList)
                {
                    if (item.ClubId == Guid.Empty)
                        throw new ArgumentException("ClubInternationalRanking must have a valid ClubId");
                }
                Save(itemList);
            }
            return itemList;
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IList<ClubInternationalRanking> Load()
        {
            if (!File.Exists(_path))
                return new List<ClubInternationalRanking>();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<ClubInternationalRanking>>(json) ?? new List<ClubInternationalRanking>();
        }

        public void Update(ClubInternationalRanking updateItem)
        {
            throw new NotImplementedException();
        }

        private void Save(IList<ClubInternationalRanking> clubInternationalRankings)
        {
            var json = JsonSerializer.Serialize(clubInternationalRankings, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }
    }
}
