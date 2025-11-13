using FootballFull.Models;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class FixtureService : IFixtureService
    {
        private IList<ClubPerCompetition> _teamsPerCompetition;
        private int _roundCount;
        private int _matchesPerRoundCount;
        private bool _alternate = false;
        private IList<int> _offsetList;
        private readonly Random rng = new Random();

        private readonly IClubService _clubService;

        public FixtureService(IClubService clubService)
        {
            _offsetList = new List<int>();
            _clubService = clubService;
        }

        public IList<Fixture> Generate(IList<ClubPerCompetition> clubsPerCompetition)
        {
            _teamsPerCompetition = clubsPerCompetition;
            Shuffle(_teamsPerCompetition);
            _roundCount = _teamsPerCompetition.Count - 1;
            _matchesPerRoundCount = _teamsPerCompetition.Count / 2;

            var firstHalfSeasonFixtures = GenerateFixtures(0);
            var secondHalfSeasonFixtures = GenerateFixtures(_teamsPerCompetition.Count - 1);

            var list = firstHalfSeasonFixtures;
            list = list.Concat(secondHalfSeasonFixtures).ToList();

            return list;
        }

        private IList<Fixture> GenerateFixtures(int roundNoOffset)
        {
            IList<Fixture> fixtures = new List<Fixture>();
            _offsetList = GenerateOffsetArray(_teamsPerCompetition.Count);

            for (int roundNo = 1; roundNo <= _roundCount; roundNo++)
            {
                _alternate = !_alternate;

                IList<int> homes = getHomes(roundNo);
                IList<int> aways = getAways(roundNo);

                for (int matchIndex = 0; matchIndex < _matchesPerRoundCount; matchIndex++)
                {
                    if (_alternate)
                    {
                        fixtures.Add(new Fixture
                        {
                            HomeTeamId = _teamsPerCompetition[homes[matchIndex]].ClubId,
                            HomeTeam = _clubService.GetClubById(_teamsPerCompetition[homes[matchIndex]].ClubId),
                            AwayTeamId = _teamsPerCompetition[aways[matchIndex]].ClubId,
                            AwayTeam = _clubService.GetClubById(_teamsPerCompetition[aways[matchIndex]].ClubId),
                            MatchDay = roundNo + roundNoOffset
                        });
                    }
                    else
                    {
                        fixtures.Add(new Fixture
                        {
                            HomeTeamId = _teamsPerCompetition[aways[matchIndex]].ClubId,
                            HomeTeam = _clubService.GetClubById(_teamsPerCompetition[aways[matchIndex]].ClubId),
                            AwayTeamId = _teamsPerCompetition[homes[matchIndex]].ClubId,
                            AwayTeam = _clubService.GetClubById(_teamsPerCompetition[homes[matchIndex]].ClubId),
                            MatchDay = roundNo + roundNoOffset
                        });
                    }

                    if (homes[matchIndex] == aways[matchIndex])
                    {
                        throw new Exception("Teams cannot play themselves");
                    }
                }
            }
            return fixtures;
        }

        private IList<int> getHomes(int roundNo)
        {
            var offset = _teamsPerCompetition.Count - roundNo;
            var array = _offsetList.ToArray();
            var homes = new ArraySegment<int>(array, offset, _matchesPerRoundCount - 1);

            var output = homes.ToList();
            output.Add(0);
            return output;
        }

        private IList<int> getAways(int roundNo)
        {
            var offset = _teamsPerCompetition.Count - roundNo + _matchesPerRoundCount - 1;
            var array = _offsetList.ToArray();
            var aways = new ArraySegment<int>(array, offset, _matchesPerRoundCount);
            var output = aways.ToArray();
            Array.Reverse(output);

            return output;
        }

        private IList<int> GenerateOffsetArray(int count)
        {
            var offsetArray = new List<int>();

            for (int i = 1; i < count; i++)
            {
                offsetArray.Add(i);
            }

            offsetArray = offsetArray.Concat(offsetArray).ToList();
            offsetArray = offsetArray.Concat(offsetArray).ToList();
            return offsetArray;
        }

        private int IsInBinarySequence(int number)
        {
            var numbertocheck = 1;
            var counter = 0;
            do
            {
                counter++;
                if (number == numbertocheck)
                {
                    return counter;
                }

                numbertocheck *= 2;
            } while (numbertocheck <= number);

            return -1;
        }
        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
