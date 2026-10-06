using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class ClubFinancialResult
    {
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }

        public decimal NetResult => Revenue - Expenses;
        public decimal DevelopmentInvestment { get; set; }
        public int StrengthBefore { get; set; }
        public int StrengthAfter { get; set; }
        public string DevelopmentReason { get; set; } = string.Empty;
        public decimal SubsidyReceived { get; set; }
    }
}
