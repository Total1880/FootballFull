using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class CompetitionPrizeMoneySettings
    {
        public decimal TotalBudget;
        public PrizeMoneyDistribution Distribution { get; set; }
    }
}
