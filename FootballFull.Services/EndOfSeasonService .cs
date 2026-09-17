using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class EndOfSeasonService : IEndOfSeasonService
    {
        private const int MaximumNumberOfClubs = 12;

        private readonly ISeasonService _seasonService;
        private readonly IClubService _clubService;
        private readonly ITrainerService _trainerService;
        private readonly ICompetitionService _competitionService;
        private readonly ICompetitionRulesService _competitionRulesService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;

        public EndOfSeasonService(
            ISeasonService seasonService,
            IClubService clubService,
            ITrainerService trainerService,
            ICompetitionService competitionService,
            ICompetitionRulesService competitionRulesService,
            IClubPerCompetitionService clubPerCompetitionService)
        {
            _seasonService = seasonService;
            _clubService = clubService;
            _trainerService = trainerService;
            _competitionService = competitionService;
            _competitionRulesService = competitionRulesService;
            _clubPerCompetitionService = clubPerCompetitionService;
        }

        public EndOfSeasonOptions GetOptions(
            Guid countryId,
            FootballAssociation footballAssociation)
        {
            var clubCount = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId)
                .Count;

            var competitions = _competitionService
                .GetCompetitionsForCountry(countryId);

            var requiredReputation = GetRequiredReputationForNextClub(clubCount);

            var canAddClub =
                clubCount < MaximumNumberOfClubs &&
                footballAssociation.Balance >= Configuration.NewClubCost &&
                footballAssociation.Reputation >= requiredReputation;

            var canCreateLowerDivision =
                clubCount >= 8 &&
                competitions.All(c => c.Tier != 2) &&
                footballAssociation.Balance >= Configuration.LowerDivisionCost &&
                footballAssociation.Reputation >= 20;

            return new EndOfSeasonOptions
            {
                CurrentClubCount = clubCount,
                CanAddClub = canAddClub,
                CanCreateLowerDivision = canCreateLowerDivision,
                CannotAddClubReason = canAddClub
                    ? null
                    : GetCannotAddClubReason(
                        clubCount,
                        requiredReputation,
                        footballAssociation),
                CannotCreateLowerDivisionReason = canCreateLowerDivision
                    ? null
                    : "Je voldoet nog niet aan de voorwaarden voor een tweede divisie."
            };
        }

        private static int GetRequiredReputationForNextClub(int clubCount)
        {
            return clubCount switch
            {
                >= 7 => 15,
                >= 6 => 10,
                _ => 0
            };
        }

        private static string GetCannotAddClubReason(
            int clubCount,
            int requiredReputation,
            FootballAssociation footballAssociation)
        {
            if (clubCount >= MaximumNumberOfClubs)
                return $"Er zijn al {MaximumNumberOfClubs} clubs.";

            if (footballAssociation.Balance < Configuration.NewClubCost)
                return "Je hebt onvoldoende saldo.";

            if (footballAssociation.Reputation < requiredReputation)
                return $"Je hebt minstens {requiredReputation} reputatie nodig.";

            return "Je kan momenteel geen club toelaten.";
        }

        public IList<Club> GetApplicantClubs(
            Guid countryId,
            FootballAssociation footballAssociation,
            int numberOfApplicants)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException(
                    "CountryId mag niet leeg zijn.",
                    nameof(countryId));

            if (footballAssociation == null)
                throw new ArgumentNullException(
                    nameof(footballAssociation));

            if (footballAssociation.CountryId != countryId)
                throw new ArgumentException(
                    "De voetbalbond behoort niet tot het opgegeven land.",
                    nameof(footballAssociation));

            if (numberOfApplicants <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(numberOfApplicants),
                    "Het aantal kandidaten moet groter zijn dan nul.");

            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanAddClub)
            {
                throw new InvalidOperationException(
                    options.CannotAddClubReason ??
                    "Er kan momenteel geen extra club worden toegelaten.");
            }

            var existingClubIds = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId)
                .Select(cpc => cpc.ClubId)
                .ToHashSet();

            return _clubService
                .GetEndOfSeasonRequestClubs(
                    countryId,
                    numberOfApplicants,
                    existingClubIds.ToList())
                .Take(numberOfApplicants)
                .ToList();
        }

        public void AdmitClub(
            Guid countryId,
            Guid clubId,
            FootballAssociation footballAssociation)
        {
            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanAddClub)
                throw new InvalidOperationException(options.CannotAddClubReason);

            var lowestCompetition = _competitionService
                .GetCompetitionsForCountry(countryId)
                .OrderByDescending(c => c.Tier)
                .First();

            _trainerService.CreateRandomTrainer(clubId);

            _clubPerCompetitionService.AddClubToCompetition(
                clubId,
                lowestCompetition.Id);

            footballAssociation.Balance -= Configuration.NewClubCost;
        }

        public void CreateLowerDivision(
            Guid countryId,
            int numberOfClubs,
            FootballAssociation footballAssociation)
        {
            var options = GetOptions(countryId, footballAssociation);

            if (!options.CanCreateLowerDivision)
                throw new InvalidOperationException(
                    options.CannotCreateLowerDivisionReason);

            if (numberOfClubs <= 0 || numberOfClubs >= options.CurrentClubCount)
                throw new ArgumentOutOfRangeException(nameof(numberOfClubs));

            var competitions = _competitionService
                .GetCompetitionsForCountry(countryId);

            var firstDivision = competitions.Single(c => c.Tier == 1);

            var ranking = _seasonService.GetRanking(firstDivision.Id);
            var clubsToMove = ranking.TakeLast(numberOfClubs).ToList();

            var secondDivision = new Competition
            {
                Id = Guid.NewGuid(),
                Name = "Division 2",
                CountryId = countryId,
                Tier = 2,
                Type = Competition.CompetitionType.League,
                Strength = Configuration.MinStrength
            };

            _competitionService.Add(secondDivision);

            _competitionRulesService.Save(new CompetitionRules
            {
                CompetitionId = firstDivision.Id,
                CompetitionRelegationToId = secondDivision.Id,
                RelegationPlaces = 1
            });

            _competitionRulesService.Save(new CompetitionRules
            {
                CompetitionId = secondDivision.Id,
                CompetitionPromotionToId = firstDivision.Id,
                PromotionPlaces = 1
            });

            foreach (var club in clubsToMove)
            {
                _clubPerCompetitionService.RemoveClubFromCompetition(
                    club.ClubId,
                    club.CompetitionId);

                _clubPerCompetitionService.AddClubToCompetition(
                    club.ClubId,
                    secondDivision.Id);
            }

            footballAssociation.Balance -= Configuration.LowerDivisionCost;
        }

        public Club CreateApplicantClub(Guid countryId, string clubName)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException(
                    "CountryId mag niet leeg zijn.",
                    nameof(countryId));

            if (string.IsNullOrWhiteSpace(clubName))
                throw new ArgumentException(
                    "De naam van de club mag niet leeg zijn.",
                    nameof(clubName));

            var normalizedName = clubName.Trim();

            var clubAlreadyExists = _clubService
                .GetClubs()
                .Any(club =>
                    club.CountryId == countryId &&
                    string.Equals(
                        club.Name,
                        normalizedName,
                        StringComparison.OrdinalIgnoreCase));

            if (clubAlreadyExists)
                throw new InvalidOperationException(
                    $"Er bestaat al een club met de naam '{normalizedName}'.");

            var club = new Club
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                CountryId = countryId,
                Strength = Configuration.MinStrength
            };

            _clubService.Add(club);

            return club;
        }
    }
}
