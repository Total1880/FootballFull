using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class SeasonService : ISeasonService
    {
        private IRepository<Competition> _competitionRepository;
        private IClubService _clubService;
        private IList<ClubLeagueCompetition> _clubLeagueCompetitions;
        private IList<ClubPerCompetition> _clubs;
        public IList<ClubLeagueCompetition> ClubLeagueCompetitions => _clubLeagueCompetitions;

        public SeasonService(IRepository<Competition> competitionRepository, IClubService clubService)
        {
            _competitionRepository = competitionRepository;
            _clubService = clubService;
        }
        public void Initialize(IList<ClubPerCompetition> clubs)
        {
            _clubs = clubs;
            InitializeNewSeason();
        }
        public void InitializeNewSeason()
        {
            RecalculateStrengths();
            PromotionsAndRelegations();
            _clubLeagueCompetitions = _clubs.Select(club => new ClubLeagueCompetition
            {
                ClubId = club.ClubId,
                Points = 0,
                GoalsFor = 0,
                GoalsAgainst = 0,
                CompetitionId = club.CompetitionId
            }).ToList();
        }

        private void RecalculateStrengths(int minStrength = 1, int maxStrength = 5)
        {
            if (_clubLeagueCompetitions == null || !_clubLeagueCompetitions.Any())
                return ;

            // Per competitie de ranking bepalen
            var competitions = _clubLeagueCompetitions
                .GroupBy(clc => clc.CompetitionId);

            foreach (var competitionGroup in competitions)
            {
                var ranked = competitionGroup
                    .OrderByDescending(c => c.Points)
                    .ThenByDescending(c => c.GoalsFor - c.GoalsAgainst)
                    .ThenByDescending(c => c.GoalsFor)
                    .ToList();

                int teamCount = ranked.Count;

                for (int i = 0; i < teamCount; i++)
                {
                    var entry = ranked[i];
                    int position = i + 1;
                    double percentile = position / (double)teamCount;

                    int delta = 0;

                    // Top 20% stijgt
                    if (percentile <= 0.20)
                        delta = +1;
                    // Onderste 20% daalt
                    else if (percentile >= 0.80)
                        delta = -1;

                    if (delta == 0)
                        continue;

                    var club = _clubService.GetClubById(entry.ClubId);
                    if (club == null)
                        continue;

                    var newStrength = club.Strength + delta;
                    if (newStrength < minStrength) newStrength = minStrength;
                    if (newStrength > maxStrength) newStrength = maxStrength;

                    club.Strength = newStrength;
                    _clubService.Update(club);
                }
            }
        }

        private void PromotionsAndRelegations()
        {
            if (_clubLeagueCompetitions == null)
                return;

            const int promotionsAndRelegationPlaces = 2;
            const int minStrength = 1;
            const int maxStrength = 5;

            var competitions = _competitionRepository.Load();

            foreach (var competition in competitions)
            {
                var clubsInCompetition = _clubLeagueCompetitions
                    .Where(_ => _.CompetitionId == competition.Id)
                    .OrderByDescending(_ => _.Points)
                    .ThenByDescending(_ => _.GoalsFor - _.GoalsAgainst)
                    .ThenByDescending(_ => _.GoalsFor)
                    .ToList();

                if (clubsInCompetition.Count == 0)
                    continue;

                var relegatedClubs = clubsInCompetition
                    .Skip(clubsInCompetition.Count - promotionsAndRelegationPlaces)
                    .ToList();

                var promotedClubs = clubsInCompetition
                    .Take(promotionsAndRelegationPlaces)
                    .ToList();

                // 1. Strength aanpassen op basis van promotie/degradatie

                // Promotie: iets sterker
                foreach (var promoted in promotedClubs)
                {
                    var club = _clubService.GetClubById(promoted.ClubId);
                    if (club == null) continue;

                    club.Strength = Math.Min(club.Strength + 1, maxStrength);
                    _clubService.Update(club);
                }

                // Degradatie: iets zwakker
                foreach (var relegated in relegatedClubs)
                {
                    var club = _clubService.GetClubById(relegated.ClubId);
                    if (club == null) continue;

                    club.Strength = Math.Max(club.Strength - 1, minStrength);
                    _clubService.Update(club);
                }

                // 2. Competitie-id updaten (promotie/degradatie zelf)

                // Promotie: naar hogere tier (lager getal)
                if (competition.Tier > 1)
                {
                    var higherCompetition = competitions
                        .FirstOrDefault(_ => _.Tier == competition.Tier - 1 && _.CountryId == competition.CountryId);

                    if (higherCompetition != null)
                    {
                        foreach (var promotedClub in promotedClubs)
                        {
                            var link = _clubs.FirstOrDefault(_ => _.ClubId == promotedClub.ClubId);
                            if (link != null)
                                link.CompetitionId = higherCompetition.Id;
                        }
                    }
                }

                // Degradatie: naar lagere tier (hoger getal)
                var maxTier = competitions.Max(_ => _.Tier);
                if (competition.Tier < maxTier)
                {
                    var lowerCompetition = competitions
                        .FirstOrDefault(_ => _.Tier == competition.Tier + 1 && _.CountryId == competition.CountryId);

                    if (lowerCompetition != null)
                    {
                        foreach (var relegatedClub in relegatedClubs)
                        {
                            var link = _clubs.FirstOrDefault(_ => _.ClubId == relegatedClub.ClubId);
                            if (link != null)
                                link.CompetitionId = lowerCompetition.Id;
                        }
                    }
                }
            }
        }

        public void PlayMatchDay(IList<Fixture> fixtures, int matchDay)
        {
            if (_clubLeagueCompetitions == null) throw new InvalidOperationException("Club league competitions not initialized.");
            foreach (var fixture in fixtures.Where(_ => _.MatchDay == matchDay))
            {
                fixture.HomeScore = Random.Shared.Next(0, 5) - fixture.AwayTeam.Strength;
                fixture.AwayScore = Random.Shared.Next(0, 5) - fixture.HomeTeam.Strength;

                while (fixture.HomeScore < 0 || fixture.AwayScore < 0)
                {
                    fixture.HomeScore++;
                    fixture.AwayScore++;
                }

                if (fixture.HomeScore > fixture.AwayScore)
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 3);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 0);
                }
                else if (fixture.HomeScore < fixture.AwayScore)
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 0);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 3);
                }
                else
                {
                    UpdateClubStats(fixture.HomeTeam.Id, fixture.HomeScore, fixture.AwayScore, 1);
                    UpdateClubStats(fixture.AwayTeam.Id, fixture.AwayScore, fixture.HomeScore, 1);
                }
            }
        }

        private void UpdateClubStats(Guid id, int awayScore, int homeScore, int v)
        {
            var record = _clubLeagueCompetitions.First(_ => _.ClubId == id);
            record.GoalsFor += awayScore;
            record.GoalsAgainst += homeScore;
            record.Points += v;
        }
    }
}
