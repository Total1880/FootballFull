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
    }
}
