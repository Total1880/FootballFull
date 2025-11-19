using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class ClubRepositoryV2 : IRepository<Club>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public ClubRepositoryV2(string path = "data/Clubs.json")
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Add(Club club)
        {
            var clubs = Load();

            // Als Id niet gevuld is, automatisch aanmaken
            if (club.Id == Guid.Empty)
                club.Id = Guid.NewGuid();

            clubs.Add(club);
            Save(clubs);
        }

        public void Update(Club updatedClub)
        {
            var clubs = Load();

            var existing = clubs.FirstOrDefault(c => c.Id == updatedClub.Id);
            if (existing == null)
                throw new Exception($"Club with ID {updatedClub.Id} not found.");

            existing.Name = updatedClub.Name;
            existing.Strength = updatedClub.Strength;
            existing.CountryId = updatedClub.CountryId;

            Save(clubs);
        }

        public void Delete(Guid id)
        {
            var clubs = Load();

            var club = clubs.FirstOrDefault(c => c.Id == id);
            if (club == null)
                throw new Exception($"Club with ID {id} not found.");

            clubs.Remove(club);
            Save(clubs);
        }

        private void Save(IList<Club> clubs)
        {
            var json = JsonSerializer.Serialize(clubs, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }

        public IList<Club> Create(IList<Club> itemList)
        {
            throw new NotImplementedException();
        }

        public IList<Club> Load()
        {
            if (!File.Exists(_path))
                return new List<Club>();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Club>>(json, _options) ?? new List<Club>();
        }
    }
}
