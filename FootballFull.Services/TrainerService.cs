using FootballFull.Models;
using FootballFull.Repositories.Interfaces;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IRepository<Trainer> _trainerRepository;
        private IList<Trainer>? _trainers;

        public TrainerService(IRepository<Trainer> trainerRepository)
        {
            _trainerRepository = trainerRepository;
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
    }
}
