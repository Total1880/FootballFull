using FootballFull.Models;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Services
{
    public class SeasonEventService : ISeasonEventService
    {
        public SeasonEvent GetRandomSeasonEvents()
        {
            var list = new List<SeasonEvent>
            {
                new SeasonEvent
                {
                    Description = "Je hebt een nieuwe sponsor gevonden!",
                    BalanceChange = 15000,
                    ReputationChange = 1
                },
                                new SeasonEvent
                {
                    Description = "Een lokaal radiostation toont interesse in je competitie!",
                    BalanceChange = 10000,
                    ReputationChange = 3
                },
                                                new SeasonEvent
                {
                    Description = "Een jeugdspeler heeft een buitenlandse transfer gemaakt!",
                    BalanceChange = 12000,
                    ReputationChange = 2
                },
                                                                new SeasonEvent
                {
                    Description = "Je hebt problemen met de fiscus ...",
                    BalanceChange = -10000,
                    ReputationChange = -1
                },
            };
            return list[new Random().Next(0, list.Count)];
        }
    }
}
