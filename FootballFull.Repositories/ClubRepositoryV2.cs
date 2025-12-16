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
            if (club == null)
                throw new ArgumentNullException(nameof(club));

            var clubs = Load();

            // Als Id niet gevuld is, automatisch aanmaken
            if (club.Id == Guid.Empty)
                club.Id = Guid.NewGuid();

            clubs.Add(club);
            Save(clubs);
        }

        public IList<Club> Create(IList<Club> itemList, bool full = false)
        {
            if (itemList == null)
                throw new ArgumentNullException(nameof(itemList));

            if (!full)
            {
                var clubs = Load();

                foreach (var club in itemList)
                {
                    if (club.Id == Guid.Empty)
                        club.Id = Guid.NewGuid();

                    clubs.Add(club);
                }

                Save(clubs);
            }
            else
            {
                // Volledig overschrijven
                foreach (var club in itemList)
                {
                    if (club.Id == Guid.Empty)
                        club.Id = Guid.NewGuid();
                }
                Save(itemList);
            }
            return itemList;
        }

        public void Update(Club updatedClub)
        {
            if (updatedClub == null)
                throw new ArgumentNullException(nameof(updatedClub));

            var clubs = Load();

            var index = clubs
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Id == updatedClub.Id)?.i;

            if (index == null)
                throw new InvalidOperationException($"Club with ID {updatedClub.Id} not found.");

            // Volledig vervangen door nieuwe versie
            clubs[index.Value] = updatedClub;
            Save(clubs);
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            var clubs = Load();

            var club = clubs.FirstOrDefault(c => c.Id == id);
            if (club == null)
                return; // of throw, afhankelijk van gewenste behavior

            clubs.Remove(club);
            Save(clubs);
        }

        public IList<Club> Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var clubs = JsonSerializer.Deserialize<List<Club>>(json, _options);
                    if (clubs != null)
                        return clubs;
                }
            }

            // Geen bestand of leeg: begin met lege lijst (clubs seed je dan via editor)
            return new List<Club>();
        }

        private void Save(IList<Club> clubs)
        {
            var json = JsonSerializer.Serialize(clubs, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }
    }
}
