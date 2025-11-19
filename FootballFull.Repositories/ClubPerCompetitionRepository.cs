using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class ClubPerCompetitionRepository : IRepository<ClubPerCompetition>
    {
        public void Add(ClubPerCompetition item)
        {
            throw new NotImplementedException();
        }

        public IList<ClubPerCompetition> Create(IList<ClubPerCompetition> itemList)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(ClubPerCompetition updateItem)
        {
            throw new NotImplementedException();
        }

        public IList<ClubPerCompetition> Load()
        {
            var list = new List<ClubPerCompetition>
            {
                new ClubPerCompetition { ClubId = Guid.Parse("978741d3-0efc-4a23-8f7a-eb8e64df8131"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("1c02bc74-7056-48a7-92c6-fcf80a6507d3"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("5f179182-6d07-4fb1-be5a-5cc72adeabff"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("b5487002-ef63-4bd2-95ae-c5499e0ec3b2"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("40d1f99e-9e49-4cec-98ad-a12ea2c3d9c6"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("ff4dbb9a-e94d-4e87-bd33-137e0a07151f"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("2601d7a7-cd55-402e-88e7-852b6a6c9442"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("03331f14-7e80-4d8c-98bf-05be39f9b90e"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("eef59f94-2fd6-412b-9da2-4cca1428799d"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("ee32cead-090c-4976-8937-61fcc05dcf0e"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("d0a02406-7fe5-44e6-8641-fbd9150a11a5"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("a984616c-1f57-4eb8-a456-7105fc032ef6"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("8bdf6ea0-4009-4b4e-b066-ba50c819892b"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("05572b00-f6f8-4c71-871f-46692d5192fe"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("8fbef4a6-1679-4b3f-9f53-b6412e98061a"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },
                new ClubPerCompetition { ClubId = Guid.Parse("7ecf9621-6956-4f81-870f-20af68b56ad1"), CompetitionId = Guid.Parse("fb60894f-07fd-4a59-aa0b-ecc16f193791") },

                new ClubPerCompetition { ClubId = Guid.Parse("787f846d-2837-4769-b297-8c9f3b25d720"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("20d7448b-d98d-4cb1-8e93-63b3fde32fba"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("e27572f6-7c24-49d1-9040-6524730c7a6d"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("8504c8ea-8613-4ea0-a4f7-32abfd4b0ff6"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("a841a0da-ad4b-4a89-9dda-7ed1ddb94157"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("68639737-23ae-4395-ae8f-dca61c646d89"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("6170ec84-1857-4d99-9c4c-618514325da1"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("c463d2cf-2781-4e5d-aa22-a1122016c5d7"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("db97d357-c901-43d9-8074-95f52be52130"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("614e2734-4a60-4fe4-829b-f34548f32016"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("6605338b-1dd8-4a50-846f-2d10b0feccbb"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("b321235d-2e7f-4c5a-9184-f53635e52b32"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("3183e5e7-1ee9-4d1a-9a22-436353bec721"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("4ba313a7-e199-47df-a2d9-4a11931c220a"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("904d950d-340e-46fd-bafd-afbe4b11240d"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("7f2ae085-52dc-4611-a97e-3983bf939485"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("a91a7dc7-9d0f-4522-a39a-2de5700d420e"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },
                new ClubPerCompetition { ClubId = Guid.Parse("01947dcd-5be0-4577-8605-d4ddc556e4f3"), CompetitionId = Guid.Parse("8cd43bfd-78e6-4272-9164-244a81811b2d") },

                new ClubPerCompetition { ClubId = Guid.Parse("6d572d64-c3e9-453a-8351-8e855f34972d"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("a3345e71-89b5-4f56-be62-3bde4ad13155"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("ed72c1df-d08d-4f69-85e6-9a718ab6dfca"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("bdc369ce-d1fa-4abc-9483-8f03c1f4d8f4"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("f4edde56-18d7-426e-abe9-e2b28ab3b1ea"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("21cf16ce-2ef4-4cdc-84e5-2aff29afd02e"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("5315a7f4-4bbe-4212-9316-2a29d2d8ba6a"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },
                new ClubPerCompetition { ClubId = Guid.Parse("7725888d-9726-4486-a846-0384b42b8273"), CompetitionId = Guid.Parse("ca43fe64-c20d-42ea-b2d3-abc24cddb54d") },


            };

            return list;
        }
    }
}
