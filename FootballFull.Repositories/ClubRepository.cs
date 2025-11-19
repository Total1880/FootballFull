using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System.Text.Json;

namespace FootballFull.Repositories
{
    public class ClubRepository : IRepository<Club>
    {
        public void Add(Club club)
        {
            throw new NotImplementedException();
        }

        public IList<Club> Create(IList<Club> itemList)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(Club updatedClub)
        {
            throw new NotImplementedException();
        }

        public IList<Club> Load()
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

            clubs.Add(new Club { Id = new Guid("787f846d-2837-4769-b297-8c9f3b25d720"), Name = "SK Beveren", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("20d7448b-d98d-4cb1-8e93-63b3fde32fba"), Name = "KV Kortrijk", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("e27572f6-7c24-49d1-9040-6524730c7a6d"), Name = "Beerschot", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("8504c8ea-8613-4ea0-a4f7-32abfd4b0ff6"), Name = "FC Liège", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("a841a0da-ad4b-4a89-9dda-7ed1ddb94157"), Name = "SK Lommel", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("68639737-23ae-4395-ae8f-dca61c646d89"), Name = "KAS Eupen", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("6170ec84-1857-4d99-9c4c-618514325da1"), Name = "Patro Eisden", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("c463d2cf-2781-4e5d-aa22-a1122016c5d7"), Name = "RWDM", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("db97d357-c901-43d9-8074-95f52be52130"), Name = "Lierse", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("614e2734-4a60-4fe4-829b-f34548f32016"), Name = "Lokeren", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("6605338b-1dd8-4a50-846f-2d10b0feccbb"), Name = "Francs Borains", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("b321235d-2e7f-4c5a-9184-f53635e52b32"), Name = "Seraing", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("3183e5e7-1ee9-4d1a-9a22-436353bec721"), Name = "Olympic Charleroi", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("4ba313a7-e199-47df-a2d9-4a11931c220a"), Name = "Sporting Hasselt", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("904d950d-340e-46fd-bafd-afbe4b11240d"), Name = "Tubize-Braine", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("7f2ae085-52dc-4611-a97e-3983bf939485"), Name = "VCO", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("a91a7dc7-9d0f-4522-a39a-2de5700d420e"), Name = "Belisia Bilzen", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });
            clubs.Add(new Club { Id = new Guid("01947dcd-5be0-4577-8605-d4ddc556e4f3"), Name = "Mons", Strength = 3, CountryId = new Guid("8cd43bfd-78e6-4272-9164-244a81811b2d") });

            clubs.Add(new Club { Id = new Guid("6d572d64-c3e9-453a-8351-8e855f34972d"), Name = "Lyra-Lierse", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("a3345e71-89b5-4f56-be62-3bde4ad13155"), Name = "Virton", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("ed72c1df-d08d-4f69-85e6-9a718ab6dfca"), Name = "Thes", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("bdc369ce-d1fa-4abc-9483-8f03c1f4d8f4"), Name = "Habay-La-Neuve", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("f4edde56-18d7-426e-abe9-e2b28ab3b1ea"), Name = "Roeselare", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("21cf16ce-2ef4-4cdc-84e5-2aff29afd02e"), Name = "Meux", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("5315a7f4-4bbe-4212-9316-2a29d2d8ba6a"), Name = "Dessel", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });
            clubs.Add(new Club { Id = new Guid("7725888d-9726-4486-a846-0384b42b8273"), Name = "Stockay-St-Georges", Strength = 3, CountryId = new Guid("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") });

            return clubs;
        }
    }
}
