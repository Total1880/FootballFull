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
    public class TrainerRepository : IRepository<Trainer>
    {
        private readonly string _path;

        public TrainerRepository(string path)
        {
            _path = path;
        }
        public void Add(Trainer item)
        {
            throw new NotImplementedException();
        }

        public IList<Trainer> Create(IList<Trainer> items)
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
    }
}
