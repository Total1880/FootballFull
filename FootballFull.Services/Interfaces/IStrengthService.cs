using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.Interfaces
{
    public interface IStrengthService
    {
        void RecalculateClubStrengths();
        void RecalculateCompetitionStrengths(int year);
    }
}
