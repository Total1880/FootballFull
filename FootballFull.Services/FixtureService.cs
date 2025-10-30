using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class FixtureService
    {
        private IList<Club> _teams;
        private int _roundCount;
        private int _matchesPerRoundCount;
        private bool _alternate = false;
        private IList<int> _offsetList;
        private readonly Random rng = new Random();

        public FixtureService()
        {
            _teams = new List<Club>();
            _offsetList = new List<int>();
        }

        public IList<Fixture> Generate(IList<Club> teams)
        {
            _teams = teams;
            Shuffle(_teams);
            _roundCount = _teams.Count - 1;
            _matchesPerRoundCount = teams.Count / 2;

            var firstHalfSeasonFixtures = GenerateFixtures(0);
            var secondHalfSeasonFixtures = GenerateFixtures(_teams.Count - 1);

            var list = firstHalfSeasonFixtures;
            list = list.Concat(secondHalfSeasonFixtures).ToList();

            return list;
        }

        private IList<Fixture> GenerateFixtures(int roundNoOffset)
        {
            IList<Fixture> fixtures = new List<Fixture>();
            _offsetList = GenerateOffsetArray(_teams.Count);

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
                            HomeTeam = _teams[homes[matchIndex]],
                            AwayTeam = _teams[aways[matchIndex]],
                            MatchDay = roundNo + roundNoOffset
                        });
                    }
                    else
                    {
                        fixtures.Add(new Fixture
                        {
                            HomeTeam = _teams[aways[matchIndex]],
                            AwayTeam = _teams[homes[matchIndex]],
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
            var offset = _teams.Count - roundNo;
            var array = _offsetList.ToArray();
            var homes = new ArraySegment<int>(array, offset, _matchesPerRoundCount - 1);

            var output = homes.ToList();
            output.Add(0);
            return output;
        }

        private IList<int> getAways(int roundNo)
        {
            var offset = _teams.Count - roundNo + _matchesPerRoundCount - 1;
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
