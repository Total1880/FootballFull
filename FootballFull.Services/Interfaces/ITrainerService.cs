using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface ITrainerService
    {
        IList<Trainer> Load();
        Trainer? GetByClubId(Guid clubId);
        bool CreateTrainers(IList<Trainer> trainers);
        Trainer CreateRandomTrainer(Guid clubId);
    }
}
