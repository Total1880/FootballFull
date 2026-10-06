using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.UI.ViewModels
{
    public class SelectSubsidyClubViewModel
    {
        public decimal SubsidyAmountA { get; init; }
        public decimal SubsidyAmountB { get; init; }
        public decimal SubsidyAmountC { get; init; }
        public int NumberOfClubs { get; init; }
        public decimal TotalSubsidyAmountA => SubsidyAmountA * NumberOfClubs;
        public decimal TotalSubsidyAmountB => SubsidyAmountB * NumberOfClubs;
        public decimal TotalSubsidyAmountC => SubsidyAmountC * NumberOfClubs;
        public decimal AssociationBalance { get; init; }
    }
}
