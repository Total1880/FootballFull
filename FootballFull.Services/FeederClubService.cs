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
    public class FeederClubService : IFeederClubService
    {
        private readonly IClubService _clubService;
        private readonly ICompetitionRulesService _competitionRulesService;
        private readonly IClubPerCompetitionService _clubPerCompetitionService;
        public FeederClubService(IClubService clubService, ICompetitionRulesService competitionRulesService, IClubPerCompetitionService clubPerCompetitionService)
        {
            _clubService = clubService;
            _competitionRulesService = competitionRulesService;
            _clubPerCompetitionService = clubPerCompetitionService;
        }
        public void AnalyzeFeederClubs()
        {
            
            var parents = _clubService.GetClubs()
                .Where(c => c.FeederClubId.HasValue)
                .ToList();
            foreach (var parent in parents)
            {
                //if(true)
                if(parent.ConsecutiveLossSeasons >= 2 && parent.Balance < Configuration.YearlyCostFeederClub)
                    {
                    _competitionRulesService.AddExtraPromotionPlace(
                        _clubPerCompetitionService.GetCompetitionsForClub(parent.FeederClubId.Value).First(_ => _.Type == CompetitionType.League).Id,
                        parent.FeederClubId.Value
                    );

                    var feederClub = _clubService.GetClubById(parent.FeederClubId.Value);
                    feederClub.IsActive = false;

                    _clubPerCompetitionService.DeleteEverythingForClub(feederClub.Id);
                    _clubService.Update(feederClub);
                }
            }
        }
    }
}
