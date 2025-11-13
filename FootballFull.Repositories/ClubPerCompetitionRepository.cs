using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Repositories
{
    public class ClubPerCompetitionRepository : IClubPerCompetitionRepository
    {
        public List<ClubPerCompetition> Load()
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

            };

            return list;
        }
    }
}
