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
    }
}
