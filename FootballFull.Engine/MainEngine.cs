using FootballFull.Models;

namespace FootballFull.Engine
{
    public class MainEngine
    {
        public List<Club> Clubs
        {
            get => _clubs;
        }
        private List<Club> _clubs { get; set; }
        private int _roundCount;
        private int _matchesPerRoundCount;
        private IList<int> _offsetList;
        private bool _alternate = false;

        public MainEngine()
        {
            _clubs = Repositories.Clubs.Load();
        }
        public void Initialize()
        {
            CreateFixtures();
        }

        private void CreateFixtures()
        {
            Shuffle(_clubs);
            _roundCount = _clubs.Count - 1;
            _matchesPerRoundCount = _clubs.Count / 2;

            var firstHalfSeasonFixtures = GenerateFixtures(0);
            var secondHalfSeasonFixtures = GenerateFixtures(_clubs.Count - 1);
        }

        private IList<Fixture> GenerateFixtures(int roundNoOffset)
        {
            IList<Fixture> fixtures = new List<Fixture>();
            _offsetList = GenerateOffsetArray(_clubs.Count);

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
                            HomeTeam = _clubs[homes[matchIndex]],
                            AwayTeam = _clubs[aways[matchIndex]],
                        });
                    }
                    else
                    {
                        fixtures.Add(new Fixture
                        {
                            HomeTeam = _clubs[aways[matchIndex]],
                            AwayTeam = _clubs[homes[matchIndex]],
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

        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
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

        private IList<int> getHomes(int roundNo)
        {
            var offset = _clubs.Count - roundNo;
            var array = _offsetList.ToArray();
            var homes = new ArraySegment<int>(array, offset, _matchesPerRoundCount - 1);

            var output = homes.ToList();
            output.Add(0);
            return output;
        }

        private IList<int> getAways(int roundNo)
        {
            var offset = _clubs.Count - roundNo + _matchesPerRoundCount - 1;
            var array = _offsetList.ToArray();
            var aways = new ArraySegment<int>(array, offset, _matchesPerRoundCount);
            var output = aways.ToArray();
            Array.Reverse(output);

            return output;
        }
    }
}
