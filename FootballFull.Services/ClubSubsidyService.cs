using FootballFull.Models;
using FootballFull.Services.Interfaces;
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
        public ClubSubsidyService(IClubService clubService)
        {
            _clubService = clubService;
        }

        public void AddSubsidy(FootballAssociation footballAssociation, decimal amountPerClub)
        {
            var clubs = _clubService.GetClubsForCountry(footballAssociation.CountryId);
            foreach (var club in clubs)
            {
                if (club.LastSeasonFinancialResult == null)
                    continue;
                club.LastSeasonFinancialResult.SubsidyReceived = amountPerClub;
                club.Balance += amountPerClub;
                club.DevelopmentBudget += amountPerClub;
                _clubService.Update(club);
            }

            footballAssociation.Balance -= (amountPerClub * clubs.Count());
        }
    }
}
