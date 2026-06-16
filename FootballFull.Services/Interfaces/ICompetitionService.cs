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
        IDictionary<int, DateTime> UpdateMatchDays(Guid competitionId, IDictionary<int, DateTime> matchDaysPerWeek);
    }
}
