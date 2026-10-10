using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly ISeasonService _seasonService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubService _clubService;
        private readonly IRepository<CompetitionSeasonHistory> _competitionSeasonHistoryRepository;
        private readonly IRepository<ClubSeasonHistory> _clubSeasonHistoryRepository;
        private IList<CompetitionSeasonHistory> _competitionSeasonHistories;
        private IList<ClubSeasonHistory> _clubSeasonHistories;

        public IList<CompetitionSeasonHistory> CompetitionSeasonHistories => _competitionSeasonHistories;
        public IList<ClubSeasonHistory> ClubSeasonHistories => _clubSeasonHistories;

        public HistoryService(
            ISeasonService seasonService,
            ICompetitionService competitionService,
            IClubService clubService,
            IRepository<CompetitionSeasonHistory> competitionSeasonHistoryRepository,
            IRepository<ClubSeasonHistory> clubSeasonHistoryRepository)
        {
            _seasonService = seasonService;
            _competitionService = competitionService;
            _clubService = clubService;
            _competitionSeasonHistoryRepository = competitionSeasonHistoryRepository;
            _clubSeasonHistoryRepository = clubSeasonHistoryRepository;

            _competitionSeasonHistories = competitionSeasonHistoryRepository.Load();
            _clubSeasonHistories = clubSeasonHistoryRepository.Load();
        }
        public void SaveSeasonHistory(
            IList<Fixture> cupFixtures,
            IList<Fixture>? internationalFixtures)
        {
            var competitions = _competitionService.GetCompetitions().Where(_ => _.Type != Competition.CompetitionType.ParentCompetition);
            foreach (var competition in competitions)
            {
                switch (competition.Type)
                {
                    case Competition.CompetitionType.League:
                        SaveLeagueHistory(competition);
                        break;

                    case Competition.CompetitionType.Cup:
                        SaveKnockoutHistory(competition, cupFixtures);
                        break;

                    case Competition.CompetitionType.International:
                        SaveKnockoutHistory(
                            competition,
                            internationalFixtures ?? new List<Fixture>());
                        break;
                }
            }
        }

        private void SaveKnockoutHistory(
            Competition competition,
            IList<Fixture> fixtures)
        {
            var competitionFixtures = fixtures
                .Where(f => f.CompetitionId == competition.Id)
                .ToList();

            if (!competitionFixtures.Any())
                return;

            var finalRound = competitionFixtures.Max(f => f.RoundNo);

            var finalFixtures = competitionFixtures
                .Where(f => f.RoundNo == finalRound)
                .ToList();

            // Voorlopig enkel finales met één wedstrijd ondersteunen.
            if (finalFixtures.Count != 1)
                return;

            var final = finalFixtures.Single();

            // Een gelijke stand vereist informatie over de winnaar
            // na verlengingen of strafschoppen.
            if (final.HomeScore == final.AwayScore)
                return;

            var winnerId = final.HomeScore > final.AwayScore
                ? final.HomeTeamId
                : final.AwayTeamId;

            var runnerUpId = final.HomeScore > final.AwayScore
                ? final.AwayTeamId
                : final.HomeTeamId;

            var winner = _clubService.GetClubById(winnerId);
            var runnerUp = _clubService.GetClubById(runnerUpId);

            if (winner == null || runnerUp == null)
                return;

            var history = new CompetitionSeasonHistory
            {
                Id = Guid.NewGuid(),
                CompetitionId = competition.Id,
                CompetitionName = competition.Name,
                Year = _seasonService.Year,
                ChampionClubId = winner.Id,
                ChampionClubName = winner.Name,
                RunnerUpClubId = runnerUp.Id,
                RunnerUpClubName = runnerUp.Name
            };

            _competitionSeasonHistoryRepository.Add(history);
            _competitionSeasonHistories.Add(history);
        }

        private void SaveLeagueHistory(Competition competition)
        {
            var ranking = _seasonService.GetRanking(competition.Id);

            var newCompetitionSeasonHistory = new CompetitionSeasonHistory
            {
                Id = Guid.NewGuid(),
                CompetitionId = competition.Id,
                CompetitionName = competition.Name,
                Year = _seasonService.Year,
                ChampionClubId = ranking[0].ClubId,
                ChampionClubName = ranking[0].Club != null ? ranking[0].Club.Name : _clubService.GetClubById(ranking[0].ClubId).Name,
                RunnerUpClubId = ranking[1].ClubId,
                RunnerUpClubName = ranking[1].Club != null ? ranking[1].Club.Name : _clubService.GetClubById(ranking[1].ClubId).Name
            };
            if (_competitionSeasonHistories.Any(_ => _.Year == _seasonService.Year && _.CompetitionId == competition.Id))
                throw new Exception("This should not be possible to add double history records");

            AddCompetitionHistoryRecords(newCompetitionSeasonHistory);

            var counterRanking = 0;

            foreach (var clubResult in ranking)
            {
                counterRanking++;
                var newClubSeasonHistory = new ClubSeasonHistory
                {
                    Id = Guid.NewGuid(),
                    ClubName = _clubService.GetClubById(clubResult.ClubId).Name,
                    CompetitionId = competition.Id,
                    CompetitionName = competition.Name,
                    ClubId = clubResult.ClubId,
                    Year = _seasonService.Year,
                    Position = counterRanking,
                    Played = clubResult.MatchesPlayed,
                    Won = clubResult.Won,
                    Drawn = clubResult.Draw,
                    Lost = clubResult.Lost,
                    GoalsFor = clubResult.GoalsFor,
                    GoalsAgainst = clubResult.GoalsAgainst,
                    Points = clubResult.Points
                };
                if (_clubSeasonHistories.Any(_ => _.Year == _seasonService.Year && _.CompetitionId == competition.Id && _.ClubId == clubResult.ClubId))
                    throw new Exception("This should not be possible to add double history records");

                AddClubHistoryRecords(newClubSeasonHistory);
            }
        }

        private void AddClubHistoryRecords(ClubSeasonHistory clubSeasonHistory)
        {
            _clubSeasonHistoryRepository.Add(clubSeasonHistory);
            _clubSeasonHistories.Add(clubSeasonHistory);
        }

        private void AddCompetitionHistoryRecords(CompetitionSeasonHistory competitionSeasonHistory)
        {
            _competitionSeasonHistoryRepository.Add(competitionSeasonHistory);
            _competitionSeasonHistories.Add(competitionSeasonHistory);
        }

        public IList<CompetitionSeasonHistory> GetHistoryForCompetitionId(Guid competitionId)
        {
            return _competitionSeasonHistories.Where(_ => _.CompetitionId == competitionId).ToList();
        }

        public IList<ClubSeasonHistory> GetHistoryForClubId(Guid clubId)
        {
            return _clubSeasonHistories.Where(_ => _.ClubId == clubId).ToList();
        }
    }
}
