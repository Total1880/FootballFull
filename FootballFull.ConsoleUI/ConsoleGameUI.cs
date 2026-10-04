using FootballFull.Services.UI;
using FootballFull.Services.UI.ViewModels;
using System.Text.RegularExpressions;

namespace FootballFull.ConsoleUI
{
    public class ConsoleGameUI : IGameUI
    {
        public void ShowFixtures(IReadOnlyList<FixtureViewModel> fixtures, bool waitForUser)
        {
            if (fixtures == null || fixtures.Count == 0)
            {
                Console.WriteLine("Geen volgende competitiewedstrijd gevonden.");
                return;
            }
            Console.WriteLine(new string('-', 40));
            Console.WriteLine("Volgende wedstrijden");
            Console.WriteLine();
            foreach (var f in fixtures)
            {
                Console.WriteLine($"{f.HomeTeamName} vs {f.AwayTeamName}");
            }
            Console.WriteLine(new string('-', 40));

            if (waitForUser)
                Console.ReadLine();
        }

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
                Console.WriteLine("[2] Andere competities");
                Console.WriteLine("[3] Internationale ranglijsten");
                Console.WriteLine("[S] Opslaan");
                Console.WriteLine("[X] Opslaan en afsluiten");
                Console.WriteLine();
                Console.Write("Maak een keuze: ");

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        return MainMenuChoice.Continue;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        return MainMenuChoice.ShowOtherCompetitions;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        return MainMenuChoice.ShowInternationalRankings;
                    case ConsoleKey.S:
                        return MainMenuChoice.Save;
                    case ConsoleKey.X:
                        return MainMenuChoice.SaveAndExit;
                }
            }
        }

        public void ShowMessage(string title, string message, bool waitForUser)
        {
            Console.Clear();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine();
            if (waitForUser)
            {
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
            }
        }

        public Guid? ChooseCompetitions(IReadOnlyList<SelectionOptionViewModel> competitions)
        {
            var counter = 0;
            Console.Clear();
            Console.WriteLine("=== Andere Competities ===");
            Console.WriteLine();

            foreach (var comp in competitions)
            {
                counter++;
                Console.WriteLine($"{counter}. {comp.Name}");
            }
            Console.WriteLine();
            Console.Write("Kies een competitie:");


            Console.WriteLine();
            var choice = Console.ReadLine();
            if (int.TryParse(choice, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= competitions.Count)
            {
                return competitions[selectedIndex - 1].Id;
            }

            return null;
        }

        public void ShowResults(IReadOnlyList<FixtureViewModel> fixtures, bool waitForUser)
        {
            if (fixtures == null || fixtures.Count == 0)
            {
                Console.WriteLine("Geen resultaten gevonden.");
                return;
            }

            int homeWidth = fixtures.Max(f => f.HomeTeamName.Length) + 2;
            int awayWidth = fixtures.Max(f => f.AwayTeamName.Length) + 2;

            Console.WriteLine(
                $"{"Home Team".PadRight(homeWidth)}" +
                $"{"Score".PadRight(8)}" +
                $"{"Away Team".PadRight(awayWidth)}"
            );

            Console.WriteLine(new string('-', homeWidth + 8 + awayWidth));

            foreach (var fixture in fixtures)
            {
                var score = $"{fixture.HomeScore} - {fixture.AwayScore}";

                Console.WriteLine(
                    $"{fixture.HomeTeamName.PadRight(homeWidth)}" +
                    $"{score.PadRight(8)}" +
                    $"{fixture.AwayTeamName.PadRight(awayWidth)}"
                );
            }

            Console.WriteLine();

            if (waitForUser)
                Console.ReadLine();
        }

        public void ShowTable(CompetitionTableViewModel competitionTable, bool waitForUser)
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

            foreach (var c in competitionTable.Rows)
            {
                Console.WriteLine(
                    $"{counter.ToString().PadRight(positionWidth)}" +
                    $"{c.ClubName.PadLeft(nameWidth)}" +
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

            if (waitForUser)
                Console.ReadLine();
        }

        public void ShowInternationRankings(IReadOnlyList<CountryRankingViewModel> rankings, int currentYear, bool waitForUser = false)
        {
            if (rankings == null || !rankings.Any())
            {
                Console.WriteLine("Geen landencoëfficiënten beschikbaar.");
                return;
            }

            // Zorg dat de lijst gesorteerd is (hoogste eerst)
            rankings = rankings
                .OrderByDescending(r => r.FiveYearCoefficient)
                .ToList();

            var years = Enumerable.Range(currentYear - 4, 5).ToList();

            Console.WriteLine("=== Country Coefficient Ranking (5-jaars) ===");
            Console.WriteLine();

            int position = 1;
            foreach (var r in rankings)
            {
                var countryName = r.CountryName;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    $"{position,2}. {countryName} – {r.FiveYearCoefficient:F2} punten");
                Console.ResetColor();

                // Detail per jaar
                foreach (var year in years.OrderByDescending(y => y))
                {
                    r.CoefficientPerYear.TryGetValue(year, out var coeff);
                    r.ClubsParticipatingPerYear.TryGetValue(year, out var clubs);
                    r.RawPointsPerYear.TryGetValue(year, out var rawPoints);

                    // bv: 2025: 7.50 (3 clubs, 22 punten)
                    Console.WriteLine(
                        $"    {year}: {coeff,6:F2} " +
                        $"({clubs} clubs, {rawPoints} punten)");
                }

                Console.WriteLine();
                position++;
            }

            Console.ReadKey();
        }

        public void ShowSeasonEvent(SeasonEventViewModel seasonEvent, bool waitForUser = false)
        {
            Console.Clear();
            Console.WriteLine(seasonEvent.Description);
            Console.WriteLine($"Balans wijziging: {seasonEvent.BalanceChange}");
            Console.WriteLine($"Reputatie wijziging: {seasonEvent.ReputationChange}");
            if (waitForUser)
                Console.ReadLine();
        }

        public void ShowSeasonFinancialResult(SeasonFinancialResultViewModel seasonFinancialResult, bool waitForUser = false)
        {
            Console.Clear();
            Console.WriteLine($"=== Season Financial Result for {seasonFinancialResult.AssociationName} ===");
            Console.WriteLine($"Club Income: {seasonFinancialResult.ClubIncome:C}");
            Console.WriteLine($"Reputation Income: {seasonFinancialResult.ReputationIncome:C}");
            Console.WriteLine($"Bonus Income: {seasonFinancialResult.BonusIncome:C}");
            Console.WriteLine();
            Console.WriteLine($"Club Costs: {seasonFinancialResult.ClubCosts:C}");
            Console.WriteLine($"Competition Costs: {seasonFinancialResult.CompetitionCosts:C}");
            Console.WriteLine($"Organisation Costs: {seasonFinancialResult.OrganisationCosts:C}");
            Console.WriteLine();
            Console.WriteLine($"Net Result: {seasonFinancialResult.NetResult:C}");
            Console.WriteLine($" Balance: {seasonFinancialResult.Balance:C}");
            Console.WriteLine();
            Console.WriteLine($"Reputation Change: {seasonFinancialResult.ReputationChange}");
            Console.WriteLine($"Reputation: {seasonFinancialResult.Reputation} ({seasonFinancialResult.ReputationDescription})");
            if( waitForUser ) Console.ReadLine();
            
        }

        public Guid AskPlayerToSelectClub(IReadOnlyList<SelectionOptionViewModel> applicants)
        {
            if (applicants == null || applicants.Count == 0)
                throw new ArgumentException(
                    "Er zijn geen kandidaat-clubs beschikbaar.",
                    nameof(applicants));

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Aanvragen van clubs ===");
                Console.WriteLine();
                Console.WriteLine(
                    "De volgende clubs willen toetreden tot de competitie:");
                Console.WriteLine();

                for (var i = 0; i < applicants.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {applicants[i].Name}");
                }

                Console.WriteLine();
                Console.Write(
                    $"Kies een club (1-{applicants.Count}): ");

                var input = Console.ReadLine();

                if (int.TryParse(input, out var selectedNumber) &&
                    selectedNumber >= 1 &&
                    selectedNumber <= applicants.Count)
                {
                    return applicants[selectedNumber - 1].Id;
                }

                Console.WriteLine();
                Console.WriteLine(
                    "Ongeldige keuze. Kies een nummer uit de lijst.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }
        }

        public bool AskYesNoQuestion(string question, bool defaultAnswer = false)
        {
            Console.WriteLine(question);
            var input = Console.ReadLine()?.ToLower();
            return input == "y" || (input == "" && defaultAnswer);
        }

        public string AskForInput(string question, string defaultAnswer = "")
        {
            Console.WriteLine(question);
            var input = Console.ReadLine();
            return string.IsNullOrEmpty(input) ? defaultAnswer : input;
        }

        public int AskForClubsToMove(int maximumClubsToMove, int currentClubCount, int minimumClubsToMove = 0)
        {
            if (maximumClubsToMove < minimumClubsToMove)
            {
                throw new InvalidOperationException(
                    "Er zijn onvoldoende clubs om twee geldige divisies te maken.");
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Lagere divisie oprichten ===");
                Console.WriteLine();
                Console.WriteLine(
                    $"Er zijn momenteel {currentClubCount} clubs.");
                Console.WriteLine(
                    "De laagst geklasseerde clubs worden naar Division 2 verplaatst.");
                Console.WriteLine();
                Console.WriteLine(
                    $"Je kan tussen {minimumClubsToMove} en " +
                    $"{maximumClubsToMove} clubs verplaatsen.");
                Console.WriteLine();

                Console.Write("Hoeveel clubs wil je verplaatsen? ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out var numberOfClubs) &&
                    numberOfClubs >= minimumClubsToMove &&
                    numberOfClubs <= maximumClubsToMove)
                {
                    return numberOfClubs;
                }

                Console.WriteLine();
                Console.WriteLine(
                    $"Voer een getal in tussen {minimumClubsToMove} " +
                    $"en {maximumClubsToMove}.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }
        }

        public bool AskToCreateLowerDivision()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Einde van het seizoen ===");
                Console.WriteLine();
                Console.WriteLine("Je kan dit seizoen:");
                Console.WriteLine();
                Console.WriteLine("[E] Een extra club toelaten");
                Console.WriteLine("[L] Een lagere divisie oprichten");
                Console.WriteLine();
                Console.Write("Maak een keuze: ");

                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.E:
                        return false;

                    case ConsoleKey.L:
                        return true;

                    default:
                        Console.WriteLine();
                        Console.WriteLine(
                            "Ongeldige keuze. Kies E of L.");
                        Console.WriteLine(
                            "Druk op een toets om opnieuw te proberen...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        public IList<string> AskStarterClubNames(int numberOfClubs)
        {
            var clubNames = new List<string>();
            Console.WriteLine("Geef de namen van de starterclubs:");
            Console.Write("Club 1: ");
            clubNames.Add(Console.ReadLine());

            Console.Write("Club 2: ");
            clubNames.Add(Console.ReadLine());

            Console.Write("Club 3: ");
            clubNames.Add(Console.ReadLine());

            Console.Write("Club 4: ");
            clubNames.Add(Console.ReadLine());

            Console.Write("Club 5: ");
            clubNames.Add(Console.ReadLine());

            Console.Write("Club 6: ");
            clubNames.Add(Console.ReadLine());
            
            return clubNames;
        }

        public int AskPlayerToSelectCountry(IReadOnlyList<SelectionOptionViewModel> countries)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Kies het land of kies 0 voor een compleet nieuw land:");
                Console.WriteLine();

                for (int i = 0; i < countries.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {countries[i].Name}");
                }

                Console.Write("\nGeef het nummer van het land: ");

                if (int.TryParse(Console.ReadLine(), out var chosenIndex) &&
                    chosenIndex >= 0 &&
                    chosenIndex <= countries.Count)
                {
                    return chosenIndex;
                }

                Console.WriteLine("Ongeldige keuze. Druk op een toets om opnieuw te proberen.");
                Console.ReadKey();
            }
        }

        public string AskNewCountryName()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Je hebt gekozen voor een compleet nieuw land.");
                Console.Write("Geef de naam van het nieuwe land: ");

                var name = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(name))
                    return name;

                Console.WriteLine("De naam mag niet leeg zijn.");
                Console.ReadKey();
            }
        }

        public void ShowNews(IReadOnlyList<NewsMessageViewModel> news)
        {
            using var enumerator = news.GetEnumerator();
            if (!enumerator.MoveNext())
                return; // geen nieuws -> meteen klaar (scheelt ook een ReadKey)

            // eerste item is er al
            do
            {
                Console.WriteLine(enumerator.Current.Message);
            }
            while (enumerator.MoveNext());

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey(true);
        }
    }
}
