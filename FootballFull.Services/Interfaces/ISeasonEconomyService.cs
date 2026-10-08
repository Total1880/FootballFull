using FootballFull.Models;

namespace FootballFull.Services.Interfaces
{
    public interface ISeasonEconomyService
    {
        SeasonFinancialResult CalculateAssociationResult(Guid countryId, int year, IList<Competition> competitions, FootballAssociation association);
        void ProcessClubFinancialResults();
        SeasonEvent ApplyRandomEvent(FootballAssociation association);
        void AllocateClubSubsidies(Guid countryId, int year, FootballAssociation association);
    }
}
