using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballFull.Services
{
    public class ClubPerCompetitionService : IClubPerCompetitionService
    {
        private readonly IClubPerCompetitionRepository _linkRepository;
        private readonly IClubService _clubService;
        private readonly ICompetitionService _competitionService;

        public ClubPerCompetitionService(
            IClubPerCompetitionRepository linkRepository,
            IClubService clubService,
            ICompetitionService competitionService)
        {
            _linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
            _clubService = clubService ?? throw new ArgumentNullException(nameof(clubService));
            _competitionService = competitionService ?? throw new ArgumentNullException(nameof(competitionService));
        }

        public void AddClubToCompetition(Guid clubId, Guid competitionId)
        {
            if (clubId == Guid.Empty) throw new ArgumentException("ClubId cannot be empty.", nameof(clubId));
            if (competitionId == Guid.Empty) throw new ArgumentException("CompetitionId cannot be empty.", nameof(competitionId));

            // optioneel: check of club/competition echt bestaan
            if (_clubService.GetClubById(clubId) == null)
                throw new InvalidOperationException($"Club {clubId} not found.");
            if (_competitionService.GetCompetitionById(competitionId) == null)
                throw new InvalidOperationException($"Competition {competitionId} not found.");

            _linkRepository.Add(new ClubPerCompetition
            {
                ClubId = clubId,
                CompetitionId = competitionId
            });
        }

        public void RemoveClubFromCompetition(Guid clubId, Guid competitionId)
        {

            _linkRepository.Delete(clubId, competitionId);

        }

        public IList<Club> GetClubsForCompetition(Guid competitionId)
        {
            var links = _linkRepository.Load()
                .Where(x => x.CompetitionId == competitionId)
                .ToList();

            var clubs = _clubService.GetClubs();

            return clubs
                .Where(c => links.Any(l => l.ClubId == c.Id))
                .ToList();
        }

        public IList<Competition> GetCompetitionsForClub(Guid clubId)
        {
            var links = _linkRepository.Load()
                .Where(x => x.ClubId == clubId)
                .ToList();

            var competitions = _competitionService.GetCompetitions();

            return competitions
                .Where(c => links.Any(l => l.CompetitionId == c.Id))
                .ToList();
        }

        public IList<ClubPerCompetition> GetAllClubPerCompetitions()
        {
            return _linkRepository.Load();
        }

        public void SaveAll(IList<ClubPerCompetition> clubPerCompetition)
        {
            _linkRepository.Create(clubPerCompetition, true);
        }

        public void UpdateInternationalCompetition(IList<ClubPerCompetition> clubPerCompetitions)
        {
            var competitions = clubPerCompetitions.Select(cpc => cpc.CompetitionId).Distinct().ToList();

            foreach (var competition in competitions)
                foreach (var club in GetAllClubPerSpecificCompetitions(competition))
                    RemoveClubFromCompetition(club.ClubId, club.CompetitionId);

            foreach (var clubPerCompetition in clubPerCompetitions)
                AddClubToCompetition(clubPerCompetition.ClubId, clubPerCompetition.CompetitionId);
        }

        public IList<ClubPerCompetition> GetAllClubPerSpecificCompetitions(Guid competitionId)
        {
            return GetAllClubPerCompetitions().Where(_ => _.CompetitionId == competitionId).ToList();
        }
    }
}
