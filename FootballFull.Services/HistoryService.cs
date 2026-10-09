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
        public void SaveSeasonHistory()
        {
            var competitions = _competitionService.GetCompetitions().Where(_ => _.Type == Competition.CompetitionType.League);
            foreach (var competition in competitions) {
                var ranking = _seasonService.GetRanking(competition.Id);

                var newCompetitionSeasonHistory = new CompetitionSeasonHistory
                {
                    Id = Guid.NewGuid(),
                    CompetitionId = competition.Id,
                    Year = _seasonService.Year,
                    ChampionClubId = ranking[0].ClubId,
                    ChampionClubName = ranking[0].Club != null ? ranking[0].Club.Name : _clubService.GetClubById(ranking[0].ClubId).Name,
                    RunnerUpClubId = ranking[1].ClubId,
                    RunnerUpClubName = ranking[1].Club != null ? ranking[1].Club.Name : _clubService.GetClubById(ranking[1].ClubId).Name
                };

                _competitionSeasonHistoryRepository.Add(newCompetitionSeasonHistory);
                _competitionSeasonHistories.Add(newCompetitionSeasonHistory);

                var counterRanking = 0;

                foreach (var clubResult in ranking)
                {
                    counterRanking++;
                    var newClubSeasonHistory = new ClubSeasonHistory
                    {
                        Id = Guid.NewGuid(),
                        CompetitionId = competition.Id,
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
                    _clubSeasonHistoryRepository.Add(newClubSeasonHistory);
                    _clubSeasonHistories.Add(newClubSeasonHistory);
                }
            }
        }
    }
}
