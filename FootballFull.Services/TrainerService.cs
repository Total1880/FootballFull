using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System.Collections.Generic;

namespace FootballFull.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IRepository<Trainer> _trainerRepository;
        private readonly IRepository<Competition> _competitionRepository;
        private readonly INameRepository _nameRepository;
        private IList<Trainer>? _trainers;
        private IList<NewsMessage> _newsMessages;
        private IList<Competition> _topLeagues;

        public IList<NewsMessage> NewsMessages => _newsMessages;

        public TrainerService(IRepository<Trainer> trainerRepository, INameRepository nameRepository, IRepository<Competition> competitionRepository)
        {
            _trainerRepository = trainerRepository;
            _competitionRepository = competitionRepository;
            _nameRepository = nameRepository;

            _newsMessages = new List<NewsMessage>();
            _topLeagues = _competitionRepository.Load().Where(c => c.Tier == 1 && c.Type == Competition.CompetitionType.League).ToList();
        }

        public IList<Trainer> Load()
        {
            _trainers ??= _trainerRepository.Load();
            return _trainers;
        }

        public Trainer? GetByClubId(Guid clubId)
        {
            if (_trainers == null)
                _trainers = _trainerRepository.Load();

            return _trainers.FirstOrDefault(t => t.ClubId == clubId);
        }

        public bool CreateTrainers(IList<Trainer> trainers)
        {
            return _trainerRepository.Create(trainers).Count == trainers.Count;
        }

        public Trainer CreateRandomTrainer(Guid clubId)
        {
            return new Trainer
            {
                Id = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                ClubId = clubId,
                Motivation = Random.Shared.Next(0, 5),
                TacticalSkill = Random.Shared.Next(0, 5),
                Name = _nameRepository.GetRandomFirstName(new Guid()),
                LastName = _nameRepository.GetRandomLastName(new Guid())
            };
        }

        public void SaveAll(IList<Trainer> trainers)
        {
            _trainerRepository.Create(trainers, true);
        }

        public Trainer NewTrainer(Guid clubId, IList<Club> clubs, IList<ClubLeagueCompetition> clubLeagueCompetitions, DateTime date)
        {
            var existingTrainer = _trainers.FirstOrDefault(_ => _.ClubId == clubId);
            if (existingTrainer != null)
                existingTrainer.ClubId = Guid.Empty;
            var newTrainer = CreateRandomTrainer(clubId);
            AssignTrainer(clubId, newTrainer, date, clubs, clubLeagueCompetitions);
            _trainers.Add(newTrainer);
            return newTrainer;
        }

        private void AssignTrainer(Guid clubId, Trainer trainer, DateTime date, IList<Club> clubs, IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            var club = clubs.First(_ => _.Id == clubId);
            club.HasTrainerSinceWeek = date;
            club.HasFiredTrainerInWeek = date;
            if (trainer.ClubId != Guid.Empty && trainer.ClubId != clubId)
            {
                var nextClub = clubs.First(_ => _.Id == trainer.ClubId);

                FindNewTrainer(nextClub, date, nextClub.Strength, trainer.Id, clubs, clubLeagueCompetitions, false);
            }
            trainer.ClubId = clubId;
        }

        public void ClubsFireTrainer(Guid userClubId, DateTime date, IList<Club> clubs, IList<ClubLeagueCompetition> clubLeagueCompetitions)
        {
            var clubsFiltered = clubs.Where(_ => _.Id != userClubId && _.Momentum < 3 && _.Morale < 3 && _.HasTrainerSinceWeek.AddDays(28) < date).ToList();

            foreach (var club in clubsFiltered)
            {
                var maxValue = 20 - ((date - club.HasTrainerSinceWeek).Days / 7);
                if (Random.Shared.Next(0, maxValue > 0 ? maxValue : 0) == 0)
                {
                    var firedTrainer = FireTrainer(club.Id, club.Strength);
                    var newTrainer = FindNewTrainer(club, date, club.Strength, firedTrainer, clubs, clubLeagueCompetitions, true);
                }
            }
        }

        private Trainer? FindNewTrainer(Club club, DateTime date, int strength, Guid firedTrainerId, IList<Club> clubs, IList<ClubLeagueCompetition> clubLeagueCompetitions, bool isFired = true)
        {
            var firedTrainer = _trainers.First(_ => _.Id == firedTrainerId);
            var message = new NewsMessage { Id = Guid.NewGuid(), ClubId = club.Id, CountryId = club.CountryId, Date = date, CompetitionId = clubLeagueCompetitions.First(_ => _.ClubId == club.Id).CompetitionId };
            if (isFired)
                message.Message = $"{club.Name} fired its trainer, {firedTrainer.Name} {firedTrainer.LastName}.";
            else message.Message = $"{club.Name} lost its trainer, {firedTrainer.Name} {firedTrainer.LastName}.";

            var trainer = FindTrainerAtOtherClub(strength, clubs, club.CountryId, clubLeagueCompetitions, date);

            if (trainer == null)
            {
                trainer = _trainers
                    .Where(_ => _.Id != firedTrainerId && _.ClubId == Guid.Empty && _.LastTeamStrength >= (strength - 5) && _.LastTeamStrength <= (strength + 5))
                    .OrderByDescending(_ => _.LastTeamStrength)
                    .FirstOrDefault();
                if (trainer != null)
                    message.Message += $"They found a new unemployed trainer, {trainer.Name} {trainer.LastName}";
            }

            if (trainer == null)
            {
                trainer = NewTrainer(club.Id, clubs, clubLeagueCompetitions, date);
                message.Message += $"They found a new trainer, {trainer.Name} {trainer.LastName}";
            }
            else
            {
                if (trainer.ClubId != Guid.Empty)
                {
                    var oldClubName = clubs.First(_ => _.Id == trainer.ClubId).Name;
                    message.Message += $"They took over trainer {trainer.Name} {trainer.LastName} from {oldClubName}";
                }
                AssignTrainer(club.Id, trainer, date, clubs, clubLeagueCompetitions);

            }

            _newsMessages.Add(message);
            return trainer;
        }

        private Trainer? FindTrainerAtOtherClub(int strength, IList<Club> clubs, Guid countryId, IList<ClubLeagueCompetition> clubLeagueCompetitions, DateTime date)
        {
            var filteredclubLeagueCompetitions = clubLeagueCompetitions
                .Where(_ => _topLeagues.Any(tl => tl.Id == _.CompetitionId))
                .ToList();

            var club = clubs
                .Where(_ => _.Strength < strength && _.Momentum > 10 && (date - _.HasTrainerSinceWeek).Days > 28 && (_.CountryId == countryId || filteredclubLeagueCompetitions.Any(f => f.ClubId == _.Id)))
                .OrderByDescending(_ => _.Strength)
                .ToList()
                .FirstOrDefault();

            return _trainers.FirstOrDefault(_ => _.ClubId == club?.Id);
        }

        private Guid FireTrainer(Guid id, int strength)
        {
            var trainer = _trainers.First(_ => _.ClubId.Equals(id));
            trainer.ClubId = Guid.Empty;
            trainer.LastTeamStrength = strength;
            return trainer.Id;
        }
    }
}
