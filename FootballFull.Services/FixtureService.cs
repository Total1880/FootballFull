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
        private readonly ICompetitionService _competitionService;

        public FixtureService(IClubService clubService, ICompetitionService competitionService)
        {
            _offsetList = new List<int>();
            _clubService = clubService;
            _competitionService = competitionService;
        }

        public IList<Fixture> Generate(IList<ClubPerCompetition> clubsPerCompetition)
        {
            var competitions = clubsPerCompetition.GroupBy(c => c.CompetitionId)
                .Select(g => g.Key)
                .ToList();
            var list = new List<Fixture>();
            foreach (var competitionId in competitions)
            {
                var competition = _competitionService.GetCompetitionById(competitionId);
                if (competition.Type != Competition.CompetitionType.League)
                    continue;
                _teamsPerCompetition = clubsPerCompetition.Where(_ => _.CompetitionId == competitionId).ToList();
                Shuffle(_teamsPerCompetition);
                _roundCount = _teamsPerCompetition.Count - 1;
                _matchesPerRoundCount = _teamsPerCompetition.Count / 2;

                var firstHalfSeasonFixtures = GenerateFixtures(0, competitionId, competition.MatchDayPerWeek);
                var secondHalfSeasonFixtures = GenerateFixtures(_teamsPerCompetition.Count - 1, competitionId, competition.MatchDayPerWeek);

                list = list.Concat(firstHalfSeasonFixtures).ToList();
                list = list.Concat(secondHalfSeasonFixtures).ToList();
            }

            return list;
        }

        private IList<Fixture> GenerateFixtures(int roundNoOffset, Guid competitionId, IDictionary<int, int> competitionWeekdays = null)
        {
            IList<Fixture> fixtures = new List<Fixture>();
            competitionWeekdays ??= new Dictionary<int, int>();

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
                        if (!competitionWeekdays.ContainsKey(roundNo + roundNoOffset))
                        {
                            competitionWeekdays = _competitionService.UpdateMatchDaysPerWeek(competitionId, new Dictionary<int, int>
                            {
                                { roundNo + roundNoOffset, roundNo + roundNoOffset }
                            });
                            
                        }
                        fixtures.Add(new Fixture
                        {
                            HomeTeamId = _teamsPerCompetition[homes[matchIndex]].ClubId,
                            HomeTeam = _clubService.GetClubById(_teamsPerCompetition[homes[matchIndex]].ClubId),
                            AwayTeamId = _teamsPerCompetition[aways[matchIndex]].ClubId,
                            AwayTeam = _clubService.GetClubById(_teamsPerCompetition[aways[matchIndex]].ClubId),
                            RoundNo = roundNo + roundNoOffset,
                            MatchDay = competitionWeekdays != null && competitionWeekdays.ContainsKey(roundNo + roundNoOffset) ? competitionWeekdays[roundNo + roundNoOffset] : roundNo + roundNoOffset,
                            CompetitionId = competitionId
                        });
                    }
                    else
                    {
                        if (!competitionWeekdays.ContainsKey(roundNo + roundNoOffset))
                        {
                            competitionWeekdays = _competitionService.UpdateMatchDaysPerWeek(competitionId, new Dictionary<int, int>
                            {
                                { roundNo + roundNoOffset, roundNo + roundNoOffset }
                            });
                        }
                        fixtures.Add(new Fixture
                        {
                            HomeTeamId = _teamsPerCompetition[aways[matchIndex]].ClubId,
                            HomeTeam = _clubService.GetClubById(_teamsPerCompetition[aways[matchIndex]].ClubId),
                            AwayTeamId = _teamsPerCompetition[homes[matchIndex]].ClubId,
                            AwayTeam = _clubService.GetClubById(_teamsPerCompetition[homes[matchIndex]].ClubId),
                            RoundNo = roundNo + roundNoOffset,
                            MatchDay = competitionWeekdays != null && competitionWeekdays.ContainsKey(roundNo + roundNoOffset) ? competitionWeekdays[roundNo + roundNoOffset] : roundNo + roundNoOffset,
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
                return new List<Fixture>();

            var fixtures = new List<Fixture>();

            // Kopie zodat we de originele lijst niet wijzigen
            var realTeams = teams.ToList();
            int n = realTeams.Count;

            // 1) Bracket size = eerstvolgende macht van 2 (bv. 5 -> 8)
            int bracketSize = 1;
            while (bracketSize < n)
                bracketSize *= 2;

            int byeCount = bracketSize - n;

            // 2) Byes aanmaken
            var byeTeams = new List<Club>();
            for (int i = 0; i < byeCount; i++)
            {
                byeTeams.Add(new Club
                {
                    Id = Guid.Empty,
                    Name = "bye"
                });
            }

            // 3) Real teams schudden (of seeden zoals je wil)
            Shuffle(realTeams);

            // 4) Eerste ronde (RoundNo = 0)
            var currentRoundFixtures = new List<Fixture>();
            int realIndex = 0;
            int byeIndex = 0;

            // 4a) Eerst alle bye-wedstrijden: real vs bye -> bye is sowieso na ronde 0 weg
            for (int i = 0; i < byeCount; i++)
            {
                var home = realTeams[realIndex++];
                var away = byeTeams[byeIndex++];

                if (competitionCup.MatchDayPerWeek == null || !competitionCup.MatchDayPerWeek.ContainsKey(1))
                {
                    competitionCup.MatchDayPerWeek = _competitionService.UpdateMatchDaysPerWeek(competitionCup.Id, new Dictionary<int, int>
                            {
                                { 1, 1 }
                            });
                }

                var fixture = new Fixture
                {
                    CompetitionId = competitionCup.Id,
                    RoundNo = 1,
                    MatchDay = competitionCup.MatchDayPerWeek == null ? 1 : competitionCup.MatchDayPerWeek[1],
                    HomeTeamId = home.Id,
                    HomeTeam = home,
                    AwayTeamId = away.Id,
                    AwayTeam = away
                };

                fixtures.Add(fixture);
                currentRoundFixtures.Add(fixture);
            }

            // 4b) Overgebleven echte teams spelen onder elkaar (geen byes meer)
            while (realIndex < realTeams.Count)
            {
                var home = realTeams[realIndex++];
                var away = realTeams[realIndex++];

                if (competitionCup.MatchDayPerWeek == null || !competitionCup.MatchDayPerWeek.ContainsKey(1))
                {
                    competitionCup.MatchDayPerWeek = _competitionService.UpdateMatchDaysPerWeek(competitionCup.Id, new Dictionary<int, int>
                            {
                                { 1, 1 }
                            });
                }

                var fixture = new Fixture
                {
                    CompetitionId = competitionCup.Id,
                    RoundNo = 1,
                    MatchDay = competitionCup.MatchDayPerWeek == null ? 1 : competitionCup.MatchDayPerWeek[1],
                    HomeTeamId = home.Id,
                    HomeTeam = home,
                    AwayTeamId = away.Id,
                    AwayTeam = away
                };

                fixtures.Add(fixture);
                currentRoundFixtures.Add(fixture);
            }

            // 5) Volgende rondes: winners vs winners, geen byes meer
            int round = 2;
            while (currentRoundFixtures.Count > 1)
            {
                var nextRoundFixtures = new List<Fixture>();

                for (int i = 0; i < currentRoundFixtures.Count; i += 2)
                {
                    if (competitionCup.MatchDayPerWeek == null || !competitionCup.MatchDayPerWeek.ContainsKey(round))
                    {
                        competitionCup.MatchDayPerWeek = _competitionService.UpdateMatchDaysPerWeek(competitionCup.Id, new Dictionary<int, int>
                            {
                                { round, round }
                            });
                    }

                    var fixture = new Fixture
                    {
                        CompetitionId = competitionCup.Id,
                        RoundNo = round,
                        MatchDay = competitionCup.MatchDayPerWeek == null ? round : competitionCup.MatchDayPerWeek[round],
                        CupPreviousFixtureHomeTeam = currentRoundFixtures[i],
                        CupPreviousFixtureAwayTeam = currentRoundFixtures[i + 1]
                    };

                    fixtures.Add(fixture);
                    nextRoundFixtures.Add(fixture);
                }

                currentRoundFixtures = nextRoundFixtures;
                round++;
            }

            return fixtures;
        }


    }
}
