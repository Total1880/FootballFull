using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class TrainerRepository : IRepository<Trainer>
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options;

        public TrainerRepository(string path)
        {
            _path = path;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
        public void Add(Trainer item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var trainers = Load();

            // Als Id niet gevuld is, automatisch aanmaken
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            trainers.Add(item);
            Save(trainers);
        }

        public IList<Trainer> Create(IList<Trainer> items, bool full)
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_path, json);

            return items;
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IList<Trainer> Load()
        {
            if (!File.Exists(_path))
                return new List<Trainer>();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Trainer>>(json) ?? new List<Trainer>();
        }

        public void Update(Trainer updateItem)
        {
            throw new NotImplementedException();
        }

        private void Save(IList<Trainer> trainers)
        {
            var json = JsonSerializer.Serialize(trainers, _options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(_path, json);
        }
    }
}
