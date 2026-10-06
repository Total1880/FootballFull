using FootballFull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IClubSubsidyService
    {
        void AddSubsidy(FootballAssociation footballAssociation, decimal amountPerClub);
    }
}
