using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ICompetitionService
    {
        void Add(Competition competition);
        void Update(Competition competition);
        void Delete(Guid id);
        IList<Competition> GetCompetitions();
        Competition? GetCompetitionById(Guid id);
        void SaveAll(IList<Competition> competitions);
        IDictionary<int, int> UpdateMatchDaysPerWeek(Guid competitionId, IDictionary<int, int> matchDaysPerWeek);
    }
}
