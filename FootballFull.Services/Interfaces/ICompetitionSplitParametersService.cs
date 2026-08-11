using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ICompetitionSplitParametersService
    {
        void Add(CompetitionSplitParameters competitionSplitParameters);
        void Update(CompetitionSplitParameters competitionSplitParameters);
        void Delete(Guid id);
        IList<CompetitionSplitParameters> GetCompetitionSplitParameters();
        CompetitionSplitParameters? GetCompetitionSplitParametersById(Guid id);
        void SaveAll(IList<CompetitionSplitParameters> competitionSplitParameters);
    }
}
