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
        private readonly IPlayerCompetitionSettingsService _playerCompetitionSettingsService;
        private readonly ICompetitionService _competitionService;
        private readonly IGameUI _gameUI;

        public EndOfSeasonChoicesService(
            IEndOfSeasonService endOfSeasonService,
            IPlayerCompetitionSettingsService playerCompetitionSettingsService,
            ICompetitionService competitionService,
            IGameUI gameUI)
        {
            _endOfSeasonService = endOfSeasonService;
            _playerCompetitionSettingsService = playerCompetitionSettingsService;
            _competitionService = competitionService;
            _gameUI = gameUI;
        }

        public bool HandleChoices(Guid countryId, FootballAssociation association)
        {
            var options = _endOfSeasonService.GetOptions(countryId, association);
            var playerSettings = _playerCompetitionSettingsService.GetSettings();

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

            if (options.CanCreateLowerDivision && (playerSettings.AutoAddCompetition || _gameUI.AskToCreateLowerDivision()))
            {
                if (playerSettings.AutoAddCompetition)
                {
                    if (playerSettings.AutoAddCompetitionThreshold < options.CurrentLowestTierClubCount - 2)
                    {
                        var numberOfClubs = options.CurrentLowestTierClubCount - playerSettings.AutoAddCompetitionThreshold;
                        _gameUI.ShowMessage("New Competition",$"{_competitionService.GetCompetitionById(_endOfSeasonService.CreateLowerDivision(countryId, numberOfClubs, association)).Name} Is now active", true);

                        return true;
                    }
                }
                else
                {
                    var numberOfClubs = _gameUI.AskForClubsToMove(
                        maximumClubsToMove: options.CurrentLowestTierClubCount - 2,
                        currentClubCount: options.CurrentLowestTierClubCount,
                        minimumClubsToMove: 2);

                    _endOfSeasonService.CreateLowerDivision(countryId, numberOfClubs, association);
                    return true;
                }
            }

            if (options.CanCreateFeederClub && (playerSettings.AutoAddFeederClub || _gameUI.AskYesNoQuestion(
    $"Wil je dat clubs een feeder club kunnen krijgen? (y/n)",
    defaultAnswer: true)))
            {
                var candidateClub = _endOfSeasonService.CandidateForFeederClub(countryId, association);
                if (candidateClub != null)
                {
                    _endOfSeasonService.AdmitClub(countryId, candidateClub.Id, association, isFeederClub: true);
                    _gameUI.ShowMessage("Feeder club aangemaakt", $"{candidateClub.Name} heeft een feeder club opgericht", true);
                }
                else
                {
                    _gameUI.ShowMessage("Geen club gevonden", "Geen kandidaat-club gevonden voor feeder club.", true);
                }
            }

            if (!options.CanAddClub)
            {
                _gameUI.ShowMessage("ERROR", options.CannotAddClubReason, true);
                return false;
            }

            if (!playerSettings.AutoAddClub && !_gameUI.AskYesNoQuestion(
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

            Guid selectedClub;
            if (!playerSettings.AutoAddClub)
                selectedClub = _gameUI.AskPlayerToSelectClub(GameUIMapper.Options(applicants));
            else
            {
                var newClub = applicants[Configuration.random.Next(0, applicants.Count())];
                selectedClub = newClub.Id;
                _gameUI.ShowMessage("Nieuwe club", $"{newClub.Name} is toegelaten in de competitie.", true);
            }

            _endOfSeasonService.AdmitClub(countryId, selectedClub, association);

            return false;
        }
    }
}
