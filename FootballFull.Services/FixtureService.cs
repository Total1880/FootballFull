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
            var competitions = clubsPerCompetition.GroupBy(c => c.CompetitionId)
                .Select(g => g.Key)
                .ToList();
            var list = new List<Fixture>();
            foreach (var competitionId in competitions)
            {
                _teamsPerCompetition = clubsPerCompetition.Where(_ => _.CompetitionId == competitionId).ToList();
                Shuffle(_teamsPerCompetition);
                _roundCount = _teamsPerCompetition.Count - 1;
                _matchesPerRoundCount = _teamsPerCompetition.Count / 2;

                var firstHalfSeasonFixtures = GenerateFixtures(0, competitionId);
                var secondHalfSeasonFixtures = GenerateFixtures(_teamsPerCompetition.Count - 1, competitionId);

                list = list.Concat(firstHalfSeasonFixtures).ToList();
                list = list.Concat(secondHalfSeasonFixtures).ToList();
            }

            return list;
        }

        private IList<Fixture> GenerateFixtures(int roundNoOffset, Guid competitionId)
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
                            MatchDay = roundNo + roundNoOffset,
                            CompetitionId = competitionId
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
                            MatchDay = roundNo + roundNoOffset,
                            CompetitionId = competitionId
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
                
                if (number == numbertocheck)
                {
                    return counter;
                }
                counter++;
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

        public IList<Fixture> GenerateCupFixtures(
            IList<ClubPerCompetition> clubsPerCompetition,
            Competition competitionCup)
        {
            if (clubsPerCompetition == null || clubsPerCompetition.Count < 2)
                return new List<Fixture>();

            // Pak de clubs voor deze competitie
            var teams = clubsPerCompetition
                .Where(cpc => cpc.CompetitionId == competitionCup.Id)
                .Select(cpc => _clubService.GetClubById(cpc.ClubId))
                .Where(c => c != null)
                .ToList();

            return GenerateCup(teams, competitionCup);
        }

        private IList<Fixture> GenerateCup(IList<Club> teams, Competition competitionCup)
        {
            if (teams == null || teams.Count < 2)
            {
                return new List<Fixture>();
            }

            int totalNumberOfTeams = teams.Count;
            var fixtures = new List<Fixture>();
            var cupround = 0;
            int counter = 0;

            if (totalNumberOfTeams % 2 != 0)
            {
                teams.Add(new Club { Id = Guid.Empty, Name = "bye" });
            }


            var extrateams = 0;
            var nextRoundTeams = totalNumberOfTeams;

            while (IsInBinarySequence(nextRoundTeams) == -1)
            {
                extrateams++;
                nextRoundTeams--;
            }

            var rounds = IsInBinarySequence(nextRoundTeams);

            //Create extra round
            if (extrateams > 0)
            {
                var extraRoundTeams = teams.OrderBy(t => t.Strength).Take(extrateams * 2).ToList();
                teams = teams.OrderBy(t => t.Strength).Skip(extrateams * 2).ToList();
                Shuffle(extraRoundTeams);
                for (int i = 0; i < extrateams * 2; i += 2)
                {
                    Fixture fixture = new Fixture();
                    counter++;

                    fixture.AwayTeamId = extraRoundTeams[i].Id;
                    fixture.AwayTeam = extraRoundTeams[i];
                    fixture.HomeTeamId = extraRoundTeams[i + 1].Id;
                    fixture.HomeTeam = extraRoundTeams[i + 1];
                    fixture.RoundNo = cupround;
                    fixture.CompetitionId = competitionCup.Id;

                    fixtures.Add(fixture);
                }
                cupround++;
                counter = 0;
            }

            Shuffle(teams);

            //Create first round
            for (int i = 0; i < nextRoundTeams /** 2 - extrateams*/; i += 2)
            {
                Fixture fixture = new Fixture();

                fixture.RoundNo = cupround;
                fixture.CompetitionId = competitionCup.Id;

                if (extrateams > i)
                {
                    fixture.CupPreviousFixtureHomeTeam = fixtures[i];
                    fixture.CupPreviousFixtureAwayTeam = fixtures[i + 1];
                }
                else
                {
                    fixture.AwayTeamId = teams[i - extrateams].Id;
                    fixture.AwayTeam = teams[i - extrateams];
                    fixture.HomeTeamId = teams[i - extrateams + 1].Id;
                    fixture.HomeTeam = teams[i - extrateams + 1];
                }
                fixtures.Add(fixture);
                counter++;
            }
            cupround++;

            while (cupround < rounds)
            {
                var roundMatches = fixtures.Where(f => f.RoundNo == cupround - 1).Count();
                var newFixtures = new List<Fixture>();
                counter = 0;

                for (int i = 0; i < roundMatches; i += 2)
                {
                    Fixture fixture = new Fixture();

                    fixture.RoundNo = cupround;
                    fixture.CompetitionId = competitionCup.Id;
                    fixture.CupPreviousFixtureHomeTeam = fixtures[fixtures.Count() - i - 1];
                    fixture.CupPreviousFixtureAwayTeam = fixtures[fixtures.Count() - i - 2];
                    newFixtures.Add(fixture);
                    counter++;
                }
                fixtures.AddRange(newFixtures);

                cupround++;
            }

            return fixtures;
        }
    }
}
