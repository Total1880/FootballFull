using FootballFull.Models;
using FootballFull.Services.Interfaces;
using FootballFull.Services.UI;
using OlavFramework;

namespace FootballFull.Services
{
    /// <summary>
    /// Coordinates player-facing decisions only. Eligibility checks and
    /// mutations remain in IEndOfSeasonService.
    /// </summary>
    public sealed class EndOfSeasonChoicesService : IEndOfSeasonChoicesService
    {
        private const int NumberOfApplicants = 3;

        private readonly IEndOfSeasonService _endOfSeasonService;
        private readonly IGameUI _gameUI;

        public EndOfSeasonChoicesService(
            IEndOfSeasonService endOfSeasonService,
            IGameUI gameUI)
        {
            _endOfSeasonService = endOfSeasonService;
            _gameUI = gameUI;
        }

        public bool HandleChoices(Guid countryId, FootballAssociation association)
        {
            var options = _endOfSeasonService.GetOptions(countryId, association);

            if (options.CanCreateNationalCup && _gameUI.AskYesNoQuestion(
                $"Wil je een nationale beker toevoegen? Kostprijs {Configuration.StartNationalCupCost} (y/n)",
                defaultAnswer: false))
            {
                _endOfSeasonService.CreateNationalCup(countryId, association);
            }

            if (!options.CanAddClub && !options.CanCreateLowerDivision)
            {
                _gameUI.ShowMessage("ERROR", options.CannotAddClubReason);
                return false;
            }

            if (options.CanCreateLowerDivision && _gameUI.AskToCreateLowerDivision())
            {
                var numberOfClubs = _gameUI.AskForClubsToMove(
                    maximumClubsToMove: options.CurrentClubCount - 2,
                    currentClubCount: options.CurrentClubCount,
                    minimumClubsToMove: 2);

                _endOfSeasonService.CreateLowerDivision(
                    countryId, numberOfClubs, association);
                return true;
            }

            if (!options.CanAddClub)
            {
                _gameUI.ShowMessage("ERROR", options.CannotAddClubReason);
                return false;
            }

            if (!_gameUI.AskYesNoQuestion(
                $"Wil je een nieuwe club toevoegen? Kostprijs {Configuration.NewClubCost} (y/n)",
                defaultAnswer: false))
            {
                return false;
            }

            var applicants = _endOfSeasonService.GetApplicantClubs(
                countryId, association, NumberOfApplicants);

            while (applicants.Count < NumberOfApplicants)
            {
                _gameUI.ShowMessage(
                    "Er zijn nog kandidaat-clubs nodig.",
                    $"Er zijn nog {NumberOfApplicants - applicants.Count} kandidaat-club(s) nodig.");

                var clubName = _gameUI.AskForInput("Geef de naam van de nieuwe club: ", "");
                if (string.IsNullOrWhiteSpace(clubName))
                {
                    _gameUI.ShowMessage("ERROR", "De naam van een club mag niet leeg zijn.");
                    continue;
                }

                var newClub = _endOfSeasonService.CreateApplicantClub(countryId, clubName);
                applicants.Add(newClub);
            }

            var selectedClub = _gameUI.AskPlayerToSelectClub(GameUIMapper.Options(applicants));
            _endOfSeasonService.AdmitClub(countryId, selectedClub, association);
            return false;
        }
    }
}
