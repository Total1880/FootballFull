using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ICompetitionService
    {
        void Add(Competition competition);
        void Update(Competition competition);
        void Delete(Guid id);
        IList<Competition> GetCompetitions(bool onlyActive = true);
        Competition? GetCompetitionById(Guid id);
        void SaveAll();
        IDictionary<int, DateTime> UpdateMatchDays(Guid competitionId, IDictionary<int, DateTime> matchDaysPerWeek);

        List<Competition> GetSubCompetitions(
Competition competition);

        IList<Competition> GetCompetitionsForCountry(Guid userCountryId, bool onlyActive = true);
        IList<Competition> GetInternationalCompetitions(bool onlyActive = true);
    }
}
