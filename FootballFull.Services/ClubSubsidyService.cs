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
    public class ClubSubsidyService : IClubSubsidyService
    {
        private readonly IClubService _clubService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        private readonly ICompetitionService _competitionService;
        private readonly IFootballAssociationsService _footballAssociationsService;
        public ClubSubsidyService(IClubService clubService, IClubPerCompetitionService clubPerCompetitionService, ICompetitionService competitionService, IFootballAssociationsService footballAssociationsService)
        {
            _clubService = clubService;
            _clubPerCompetitionService = clubPerCompetitionService;
            _competitionService = competitionService;
            _footballAssociationsService = footballAssociationsService;
        }

        public void AddSubsidy(
            FootballAssociation association,
            decimal amountPerClub,
            int seasonYear)
        {
            if (amountPerClub < 0)
                throw new ArgumentOutOfRangeException(nameof(amountPerClub));

            if (association.LastSubsidySeason == seasonYear)
                return;

            var clubs = GetEligibleClubs(association.CountryId);
            var totalCost = amountPerClub * clubs.Count;

            if (totalCost > Math.Max(-Configuration.AssociationMaxDebt, association.Balance))
                throw new InvalidOperationException(
                    "De voetbalbond heeft onvoldoende geld voor deze subsidie.");

            foreach (var club in clubs)
            {
                club.Balance += amountPerClub;
                club.SubsidySeason = seasonYear;
                club.SeasonSubsidyReceived = amountPerClub;

                _clubService.Update(club);
            }

            association.Balance -= totalCost;
            association.LastSubsidySeason = seasonYear;
            association.LastSubsidyTotalCost = totalCost;

            _footballAssociationsService.Update(association);
        }

        public IList<Club> GetEligibleClubs(Guid countryId)
        {
            var leagueIds = _competitionService
                .GetCompetitionsForCountry(countryId)
                .Where(c => c.Type == Competition.CompetitionType.League)
                .Select(c => c.Id)
                .ToHashSet();

            var clubIds = _clubPerCompetitionService
                .GetAllClubPerCompetitionForCountry(countryId)
                .Where(link => leagueIds.Contains(link.CompetitionId))
                .Select(link => link.ClubId)
                .ToHashSet();

            return _clubService.GetClubs()
                .Where(c => c.CountryId == countryId && clubIds.Contains(c.Id))
                .ToList();
        }
    }
}
