using FootballFull.Services.UI;
using FootballFull.Services.UI.ViewModels;

namespace FootballFull.ConsoleUI
{
    public class ConsoleGameUI : IGameUI
    {
        public MainMenuChoice ShowMainMenu(GameDashboardViewModel dashboard)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {dashboard.AssociationName} ===");
                Console.WriteLine($"Seizoen {dashboard.SeasonStartYear}/{dashboard.SeasonStartYear + 1}");
                Console.WriteLine($"Datum: {dashboard.CurrentDate:dd/MM/yyyy}");
                Console.WriteLine();
                Console.WriteLine($"Saldo: {dashboard.Balance:C}");
                Console.WriteLine($"Reputatie: {dashboard.Reputation} - {dashboard.ReputationDescription}");
                Console.WriteLine($"Clubs: {dashboard.ClubCount}");
                Console.WriteLine($"Competities: {dashboard.CompetitionCount}");
                Console.WriteLine();

                if (dashboard.NextMatchday.HasValue)
                {
                    Console.WriteLine(
                        $"Volgende speeldag: {dashboard.NextMatchday:dd/MM/yyyy} " +
                        $"({dashboard.NextMatchCount} wedstrijden)");
                }
                else
                {
                    Console.WriteLine("Geen volgende speeldag gepland.");
                }

                Console.WriteLine();
                Console.WriteLine("[1] Verder naar volgende speeldag");
                Console.WriteLine("[S] Opslaan");
                Console.WriteLine("[X] Opslaan en afsluiten");
                Console.WriteLine();
                Console.Write("Maak een keuze: ");

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        return MainMenuChoice.Continue;
                    case ConsoleKey.S:
                        return MainMenuChoice.Save;
                    case ConsoleKey.X:
                        return MainMenuChoice.SaveAndExit;
                }
            }
        }

        public void ShowMessage(string title, string message)
        {
            Console.Clear();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
        }

        public void ShowTable(CompetitionTableViewModel competitionTable)
        {
            const int positionWidth = 4;
            const int nameWidth = 25;
            const int gamesWidth = 8;
            const int wonWidth = 8;
            const int drawWidth = 8;
            const int lostWidth = 8;
            const int gfWidth = 6;
            const int gaWidth = 6;
            const int gdWidth = 6;
            const int pointsWidth = 8;

            Console.Clear();
            Console.WriteLine($"=== League Table: {competitionTable.CompetitionName} ===");
            Console.WriteLine();

            Console.WriteLine(
    $"{"P".PadRight(positionWidth)}" +
    $"{"Club".PadRight(nameWidth)}" +
    $"{"Games".PadLeft(gamesWidth)}" +
    $"{"Won".PadLeft(wonWidth)}" +
    $"{"Draw".PadLeft(drawWidth)}" +
    $"{"Lost".PadLeft(lostWidth)}" +
    $"{"GF".PadLeft(gfWidth)}" +
    $"{"GA".PadLeft(gaWidth)}" +
    $"{"GD".PadLeft(gdWidth)}" +
    $"{"Points".PadLeft(pointsWidth)}"
);
            Console.WriteLine(new string('-', positionWidth + nameWidth + gamesWidth + wonWidth + drawWidth + lostWidth + pointsWidth + gfWidth + gaWidth + gdWidth));

            var counter = 1;

            foreach (var c in competitionTable.ClubLeagueCompetitions)
            {
                Console.WriteLine(
                    $"{counter.ToString().PadRight(positionWidth)}" +
                    $"{c.Club.Name.PadLeft(nameWidth)}" +
                    $"{c.MatchesPlayed.ToString().PadLeft(gamesWidth)}" +
                    $"{c.Won.ToString().PadLeft(wonWidth)}" +
                    $"{c.Draw.ToString().PadLeft(drawWidth)}" +
                    $"{c.Lost.ToString().PadLeft(lostWidth)}" +
                    $"{c.GoalsFor.ToString().PadLeft(gfWidth)}" +
                    $"{c.GoalsAgainst.ToString().PadLeft(gaWidth)}" +
                    $"{c.GoalDifference.ToString().PadLeft(gdWidth)}" +
                    $"{c.Points.ToString().PadLeft(pointsWidth)}"
                );

                Console.ResetColor();
                counter++;
            }

            Console.ReadLine();
        }
    }
}
