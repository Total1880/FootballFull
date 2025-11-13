using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class ClubRepository : IClubRepository
    {
        public List<Club> Load(string path = "data/Clubs.json")
        {
            var clubs = new List<Club>();
            clubs.Add(new Club{Id = Guid.NewGuid(), Name = "Union SG", Strength = 3});
            clubs.Add(new Club{Id = Guid.NewGuid(), Name = "Club Brugge", Strength = 3});
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Anderlecht", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Sint-Truiden", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "AA Gent", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "KV Mechelen", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "RC Genk", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Zulte Waregem", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Charleroi", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Standard", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "RAAL La Louvière", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Westerlo", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "OH Leuven", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Anderlecht", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Anderlecht", Strength = 3 });
            clubs.Add(new Club { Id = Guid.NewGuid(), Name = "Anderlecht", Strength = 3 });
            return clubs;
        }
    }
}
