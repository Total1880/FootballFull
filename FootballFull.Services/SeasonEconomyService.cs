using FootballFull.Models;
using FootballFull.Services.Interfaces;
using FootballFull.Services.UI;
using FootballFull.Services.UI.ViewModels;

namespace FootballFull.Services
{
    /// <summary>
    /// Coordinates seasonal financial operations; calculation rules remain in
    /// the existing financial, subsidy and end-of-season services.
    /// </summary>
    public sealed class SeasonEconomyService : ISeasonEconomyService
    {
        private readonly ISeasonFinancialResultService _financialResults;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly IFootballAssociationsService _associations;
        private readonly ICountryService _countries;
        private readonly IEndOfSeasonService _endOfSeason;
        private readonly ISeasonEventService _seasonEvents;
        private readonly IClubSubsidyService _subsidies;
        private readonly IGameUI _gameUI;

        public SeasonEconomyService(
            ISeasonFinancialResultService financialResults,
            IClubPerCompetitionService clubPerCompetitionService,
            IFootballAssociationsService associations,
            ICountryService countries,
            IEndOfSeasonService endOfSeason,
            ISeasonEventService seasonEvents,
            IClubSubsidyService subsidies,
            IGameUI gameUI)
        {
            _financialResults = financialResults;
            _clubPerCompetitionService = clubPerCompetitionService;
            _associations = associations;
            _countries = countries;
            _endOfSeason = endOfSeason;
            _seasonEvents = seasonEvents;
            _subsidies = subsidies;
            _gameUI = gameUI;
        }

        public SeasonFinancialResult CalculateAssociationResult(
            Guid countryId,
            int year,
            IList<Competition> competitions,
            FootballAssociation association)
        {
            var clubCount = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId).Count;

            var competitionCount = competitions.Count(c => c.CountryId == countryId);
            var subsidyCosts = association.LastSubsidySeason == year
                ? association.LastSubsidyTotalCost
                : 0m;

            var result = _financialResults.CalculateFinancialResults(
                clubCount, competitionCount, association, subsidyCosts);

            _associations.Update(association);
            return result;
        }

        public void ProcessClubFinancialResults()
        {
            foreach (var country in _countries.GetCountries())
                _endOfSeason.ProcessClubFinances(country.Id);
        }

        public SeasonEvent ApplyRandomEvent(FootballAssociation association)
        {
            var seasonEvent = _seasonEvents.GetRandomSeasonEvents();
            association.Balance += seasonEvent.BalanceChange;
            association.Reputation += seasonEvent.ReputationChange;
            _associations.Update(association);
            return seasonEvent;
        }

        public void AllocateClubSubsidies(Guid countryId, int year, FootballAssociation association)
        {
            var amount = _gameUI.AskForSubsidyAmount(new SelectSubsidyClubViewModel
            {
                SubsidyAmountA = 0,
                SubsidyAmountB = 5000,
                SubsidyAmountC = 10000,
                NumberOfClubs = _subsidies.GetEligibleClubs(countryId).Count,
                AssociationBalance = association.Balance
            });

            try
            {
                _subsidies.AddSubsidy(association, amount, year);
            }
            catch (InvalidOperationException ex)
            {
                _gameUI.ShowMessage("Error", ex.Message);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
