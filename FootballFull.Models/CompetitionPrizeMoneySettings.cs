using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class CompetitionPrizeMoneySettings
    {
        public decimal TotalBudget { get; set; }
        public PrizeMoneyDistribution Distribution { get; set; }
    }
}
