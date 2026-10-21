using FootballFull.Models;
using FootballFull.Services.Interfaces;
using OlavFramework;
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
        private readonly IInternationalRankingService _clubInternationalRankingService;
        private readonly IInternationalCompetitionSettingsService _internationalCompetitionSettingsService;
        private IList<InternationalCompetitionQualificationPlaces> _qualificationPlaces;

        public InternationalQualificationPlacesService(
            ICompetitionService competitionService,
            ICountryService countryService,
            IInternationalRankingService clubInternationalRankingService,
            IInternationalCompetitionSettingsService internationalCompetitionSettingsService)
        {
            _competitionService = competitionService;
            _countryService = countryService;
            _clubInternationalRankingService = clubInternationalRankingService;
            _internationalCompetitionSettingsService = internationalCompetitionSettingsService;

        }
        public void CalculateQualificationPlaces(int year)
        {
            var countries = _countryService.GetCountries();
            var ranking = _clubInternationalRankingService.GetAll();
            var internationalCompetitions = _competitionService.GetInternationalCompetitions();

            _qualificationPlaces = new List<InternationalCompetitionQualificationPlaces>();

            foreach (var competition in internationalCompetitions)
            {
                var tempRanking = new Dictionary<Guid, decimal>();

                if (!_qualificationPlaces.Any(_ => _.CompetitionId == competition.Id))
                    _qualificationPlaces.Add(new InternationalCompetitionQualificationPlaces { CompetitionId = competition.Id, Year = year });

                var settings = _internationalCompetitionSettingsService.GetSettings(competition.Id);
                var counter = 0;

                foreach (var country in countries)
                    tempRanking.Add(country.Id, _clubInternationalRankingService.GetCountryPointsForXYears(country.Id, year, Configuration.InternationalRankingYears));

                foreach (var country in tempRanking.OrderByDescending(_ => _.Value))
                {
                    var places = settings.QualificationPlaces.Count() > counter ? settings.QualificationPlaces[counter] : Configuration.DefaultQualificationPlaces;
                    _qualificationPlaces.First(_ => _.CompetitionId == competition.Id).Rows.Add(new InternationalCompetitionQualificationPlacesRow
                    {
                        CountryId = country.Key,
                        Places = places
                    });

                    counter++;
                }
            }
        }

        public int GetQualificationPlacesForCountryAndCompetition(Guid countryId, Guid competitionId)
        {
            if (!_qualificationPlaces.Any(_ => _.CompetitionId == competitionId))
                throw new NotImplementedException("Competition is not implemented yet");

            return _qualificationPlaces.First(_ => _.CompetitionId == competitionId).Rows.First(_ => _.CountryId == countryId).Places;
        }
    }
}
