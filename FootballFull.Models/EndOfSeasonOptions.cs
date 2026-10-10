using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class EndOfSeasonOptions
    {
        public bool CanAddClub { get; init; }
        public bool CanCreateLowerDivision { get; init; }
        public bool CanCreateNationalCup { get; init; }

        public string? CannotAddClubReason { get; init; }
        public string? CannotCreateLowerDivisionReason { get; init; }
        public string? CannotCreateNationalCupReason { get; init; }

        public int CurrentClubCount { get; init; }
    }
}
