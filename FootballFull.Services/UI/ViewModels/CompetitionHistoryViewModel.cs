using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services.UI.ViewModels
{
    public class CompetitionHistoryViewModel
    {
        public IReadOnlyList<CompetitionHistoryRowViewModel> Rows { get; init; } = Array.Empty<CompetitionHistoryRowViewModel>();
    }
}
