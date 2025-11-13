using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class ClubRepository : IClubRepository
    {
        public List<Club> Load(string path = "data/Clubs.json")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing clubs file at '{path}'.");

            var json = File.ReadAllText(path);
            var clubs = JsonSerializer.Deserialize<List<Club>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<Club>();

            return clubs;
        }
    }
}
