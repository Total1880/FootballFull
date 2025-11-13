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
            clubs.Add(new Club { Id = new Guid("978741d3-0efc-4a23-8f7a-eb8e64df8131"), Name = "Union SG", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("1c02bc74-7056-48a7-92c6-fcf80a6507d3"), Name = "Club Brugge", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("5f179182-6d07-4fb1-be5a-5cc72adeabff"), Name = "Anderlecht", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("b5487002-ef63-4bd2-95ae-c5499e0ec3b2"), Name = "Sint-Truiden", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("40d1f99e-9e49-4cec-98ad-a12ea2c3d9c6"), Name = "AA Gent", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("ff4dbb9a-e94d-4e87-bd33-137e0a07151f"), Name = "KV Mechelen", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("2601d7a7-cd55-402e-88e7-852b6a6c9442"), Name = "RC Genk", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("03331f14-7e80-4d8c-98bf-05be39f9b90e"), Name = "Zulte Waregem", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("eef59f94-2fd6-412b-9da2-4cca1428799d"), Name = "Charleroi", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("ee32cead-090c-4976-8937-61fcc05dcf0e"), Name = "Standard", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("d0a02406-7fe5-44e6-8641-fbd9150a11a5"), Name = "RAAL La Louvière", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("a984616c-1f57-4eb8-a456-7105fc032ef6"), Name = "Westerlo", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("8bdf6ea0-4009-4b4e-b066-ba50c819892b"), Name = "OH Leuven", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("05572b00-f6f8-4c71-871f-46692d5192fe"), Name = "Antwerp", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("8fbef4a6-1679-4b3f-9f53-b6412e98061a"), Name = "Cercle Brugge", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            clubs.Add(new Club { Id = new Guid("7ecf9621-6956-4f81-870f-20af68b56ad1"), Name = "Dender", Strength = 3, CountryId = new Guid("7a7efcb7-f4c5-477c-9210-b897dc7f00f1") });
            return clubs;
        }
    }
}
