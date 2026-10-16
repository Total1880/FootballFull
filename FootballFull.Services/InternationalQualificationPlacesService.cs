using FootballFull.Models;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballFull.Models.Competition;

namespace FootballFull.Services
{
    public class InternationalQualificationPlacesService : IInternationalQualificationPlacesService
    {
        private readonly ICompetitionService _competitionService;
        private readonly ICountryService _countryService;
        private readonly IClubInternationalRankingService _clubInternationalRankingService;
        private readonly IInternationalCompetitionSettingsService _internationalCompetitionSettingsService;
        private IList<InternationalQualificationPlaces> _qualificationPlaces;

        public InternationalQualificationPlacesService(
            ICompetitionService competitionService, 
            ICountryService countryService, 
            IClubInternationalRankingService clubInternationalRankingService,
            IInternationalCompetitionSettingsService internationalCompetitionSettingsService)
        {
            _competitionService = competitionService;
            _countryService = countryService;
            _clubInternationalRankingService = clubInternationalRankingService;
            _internationalCompetitionSettingsService = internationalCompetitionSettingsService;
            _qualificationPlaces = new List<InternationalQualificationPlaces>();
        }
        public void CalculateQualificationPlaces()
        {
            var countries = _countryService.GetCountries();
            var ranking = _clubInternationalRankingService.GetAll();
            var internationalCompetitions = _competitionService.GetInternationalCompetitions();

            foreach (var competition in internationalCompetitions)
            {

            }
        }
    }
}
