using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;

namespace FootballFull.Services
{
    public class ClubInternationalRankingService : IClubInternationalRankingService
    {
        private IList<ClubInternationalRanking> _rankings;
        private readonly IRepository<ClubInternationalRanking> _repository;
        public ClubInternationalRankingService(IRepository<ClubInternationalRanking> repository)
        {
            _repository = repository;
            _rankings = _repository.Load();
        }
        public IList<ClubInternationalRanking> GetAll()
        {
            return _rankings;
        }

        public void SaveAll(IList<ClubInternationalRanking> clubs)
        {
            _repository.Create(clubs, true);
            _rankings = clubs;
        }

        public ClubInternationalRanking UpdatePoints(Guid clubId, Guid CompetitionId, int year, int point)
        {
            var ranking = _rankings.Where(r => r.ClubId == clubId && r.CountryId == CompetitionId).ToList();

            if (ranking.Count == 0)
            {
                var newRanking = new ClubInternationalRanking
                {
                    ClubId = clubId,
                    CountryId = CompetitionId
                };
                newRanking.PointsPerYear[year] = point;
                _rankings.Add(newRanking);
                return newRanking;
            }
            else if (ranking.Count > 1)
            {
                throw new Exception($"Multiple rankings found for ClubId: {clubId} and CompetitionId: {CompetitionId}");
            }
            else
            {
                ranking[0].PointsPerYear[year] += point;
                return ranking[0];
            }
        }
    }
}
