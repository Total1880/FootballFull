using FootballFull.Models;
using FootballFull.Services.UI;
using FootballFull.Services.UI.ViewModels;
using OlavFramework;
using System.Globalization;
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
                Console.WriteLine("============== FOOTBALL FULL ==============");
                Console.WriteLine($"Verbond: {dashboard.AssociationName}");
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
                Console.WriteLine("SPELEN");
                Console.WriteLine("[1] Verder naar volgende speeldag");
                Console.WriteLine();
                Console.WriteLine("COMPETITIES & STATISTIEKEN");
                Console.WriteLine("[2] Andere competities");
                Console.WriteLine("[3] Internationale ranglijsten");
                Console.WriteLine("[4] Competitiegeschiedenis");
                Console.WriteLine("[5] Clubgeschiedenis");
                Console.WriteLine();
                Console.WriteLine("BEHEER");
                Console.WriteLine("[6] Instellingen");
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
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        return MainMenuChoice.ShowCompetitionHistory;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        return MainMenuChoice.ShowClubHistory;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        return MainMenuChoice.Settings;
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

        private static Guid? ChooseOption(string title, IReadOnlyList<SelectionOptionViewModel> options)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {title} ===");
                Console.WriteLine();
                if (options == null || options.Count == 0)
                {
                    Console.WriteLine("Geen opties beschikbaar.");
                    Console.WriteLine("Druk op een toets om terug te gaan...");
                    Console.ReadKey(true);
                    return null;
                }

                for (int i = 0; i < options.Count; i++)
                    Console.WriteLine($"[{i + 1}] {options[i].Name}");

                Console.WriteLine();
                Console.WriteLine("[0] Terug");
                Console.Write("Maak een keuze: ");
                var input = Console.ReadLine();
                if (input == null || input.Trim() == "0")
                    return null;
                if (int.TryParse(input, out var number) && number >= 1 && number <= options.Count)
                    return options[number - 1].Id;

                Console.WriteLine("Ongeldige keuze. Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }
        }

        public Guid? ChooseClub(IReadOnlyList<SelectionOptionViewModel> clubs)
            => ChooseOption("Clubs", clubs);

        public Guid? ChooseCompetitions(IReadOnlyList<SelectionOptionViewModel> competitions)
            => ChooseOption("Competities", competitions);

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
                $"{"Thuisploeg".PadRight(homeWidth)}" +
                $"{"Score".PadRight(8)}" +
                $"{"Uitploeg".PadRight(awayWidth)}"
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
            var rows = competitionTable.Rows.Select((c, index) => (IReadOnlyList<string>)new[]
            {
                (index + 1).ToString(), c.ClubName, c.MatchesPlayed.ToString(),
                c.Won.ToString(), c.Draw.ToString(), c.Lost.ToString(),
                c.GoalsFor.ToString(), c.GoalsAgainst.ToString(), c.GoalDifference.ToString(),
                c.Points.ToString()
            }).ToList();
            ConsoleUIHelper.Table($"Klassement - {competitionTable.CompetitionName}",
                new[] { "#", "Club", "M", "W", "G", "V", "DV", "DT", "DS", "Ptn" }, rows, waitForUser);
        }

        public void ShowInternationRankings(
            IReadOnlyList<CountryRankingViewModel> rankings,
            int currentYear,
            bool showDetails,
             bool waitForUser)
        {
            if (rankings == null || rankings.Count == 0)
            {
                Console.WriteLine("No international rankings available.");
                return;
            }

            int years = Configuration.InternationalRankingYears;
            int startYear = currentYear - years + 1;

            var yearRange = Enumerable.Range(startYear, years).ToList();

            Console.WriteLine();
            Console.WriteLine($"INTERNATIONAL COUNTRY RANKING - {currentYear}");
            Console.WriteLine();

            Console.Write($"{"Pos",-5} {"Country",-20}");

            foreach (int year in yearRange)
                Console.Write($"{year,10}");

            Console.WriteLine($"{"Total",12}");

            Console.WriteLine(new string('-', 5 + 20 + years * 10 + 12));

            int position = 1;

            foreach (var ranking in rankings.OrderByDescending(r => r.FiveYearCoefficient))
            {
                Console.Write($"{position,-5} {ranking.CountryName,-20}");

                foreach (int year in yearRange)
                {
                    double coefficient = ranking.CoefficientPerYear
                        .GetValueOrDefault(year);

                    Console.Write($"{coefficient,10:F3}");
                }

                Console.WriteLine($"{ranking.FiveYearCoefficient,12:F3}");

                if (showDetails)
                {
                    Console.Write($"{"",-5} {"Clubs",-20}");

                    foreach (int year in yearRange)
                    {
                        int clubs = ranking.ClubsParticipatingPerYear
                            .GetValueOrDefault(year);

                        Console.Write($"{clubs,10}");
                    }

                    Console.WriteLine();

                    Console.Write($"{"",-5} {"Raw Points",-20}");

                    foreach (int year in yearRange)
                    {
                        int points = ranking.RawPointsPerYear
                            .GetValueOrDefault(year);

                        Console.Write($"{points,10}");
                    }

                    Console.WriteLine();
                }

                position++;
            }

            Console.WriteLine();
            if (waitForUser)
                Console.ReadLine();
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
            Console.WriteLine($"Subsidy Costs: {seasonFinancialResult.SubsidyCosts:C}");
            Console.WriteLine($"Prize Money Costs: {seasonFinancialResult.PrizeMoneyCosts:C}");
            Console.WriteLine();
            Console.WriteLine($"Net Result: {seasonFinancialResult.NetResult:C}");
            Console.WriteLine($" Balance: {seasonFinancialResult.Balance:C}");
            Console.WriteLine();
            Console.WriteLine($"Reputation Change: {seasonFinancialResult.ReputationChange}");
            Console.WriteLine($"Reputation: {seasonFinancialResult.Reputation} ({seasonFinancialResult.ReputationDescription})");
            if (waitForUser) Console.ReadLine();

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

            Console.Clear();
            Console.WriteLine("Geef de namen van de starterclubs:");

            for (var i = 1; i <= numberOfClubs; i++)
            {
                string? clubName;

                do
                {
                    Console.Write($"Club {i}: ");
                    clubName = Console.ReadLine()?.Trim();
                }
                while (string.IsNullOrWhiteSpace(clubName));

                clubNames.Add(clubName);
            }

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

        public void ShowClubDevelopment(IReadOnlyList<ClubDevelopmentViewModel> clubs, bool waitForUser = false)
        {
            Console.Clear();
            Console.WriteLine("=== Clubfinanciën en ontwikkeling ===");
            Console.WriteLine("Een deel van de winst wordt gereserveerd voor ontwikkeling; investeren kost geld.");
            Console.WriteLine();
            foreach (var club in clubs)
            {
                Console.WriteLine($"{club.ClubName}: strength {club.StrengthBefore} -> {club.StrengthAfter}");
                Console.WriteLine($"  Resultaat: {club.NetResult:N0} | Investering: {club.Investment:N0} | Saldo: {club.Balance:N0}");
                Console.WriteLine($"  Ontvangen prijzengeld: {club.PrizeMoneyReceived:N0}");
                Console.WriteLine($"  Gereserveerd budget: {club.DevelopmentBudget:N0} | {club.Reason}");
                if (club.CreateFeederClubCost > 0)
                    Console.WriteLine($"  Kosten voor aanleggen feederclub: {club.CreateFeederClubCost:N0}");
                Console.WriteLine();
            }
            if (waitForUser)
            {
                Console.WriteLine("Druk op een toets om verder te gaan.");
                Console.ReadKey(true);
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

            ConsoleUIHelper.Pause();
        }

        public decimal AskForSubsidyAmount(SelectSubsidyClubViewModel viewModel)
        {
            ConsoleUIHelper.Header("Subsidie");
            Console.WriteLine($"Beschikbaar saldo: {viewModel.AssociationBalance:N0}");
            Console.WriteLine($"[1] {viewModel.SubsidyAmountA:N0} (totale kost: {viewModel.TotalSubsidyAmountA:N0})");
            Console.WriteLine($"[2] {viewModel.SubsidyAmountB:N0} (totale kost: {viewModel.TotalSubsidyAmountB:N0})");
            Console.WriteLine($"[3] {viewModel.SubsidyAmountC:N0} (totale kost: {viewModel.TotalSubsidyAmountC:N0})");
            return ConsoleUIHelper.ReadInt("Kies een subsidie", 1, 3) switch
            {
                1 => viewModel.SubsidyAmountA,
                2 => viewModel.SubsidyAmountB,
                _ => viewModel.SubsidyAmountC
            };
        }

        public void ShowCompetitionHistory(CompetitionHistoryViewModel history)
        {
            var rows = history.Rows.OrderByDescending(x => x.Year).ThenBy(x => x.CompetitionName)
                .Select(x => (IReadOnlyList<string>)new[]
                {
                    x.Year.ToString(), x.CompetitionName ?? "-", x.ChampionClubName ?? "-", x.RunnerUpClubName ?? "-"
                }).ToList();
            ConsoleUIHelper.Table("Competitiegeschiedenis",
                new[] { "Seizoen", "Competitie", "Kampioen", "Tweede" }, rows);
        }

        public void ShowClubHistory(ClubHistoryViewModel history)
        {
            var rows = history.ClubHistory.OrderByDescending(x => x.Year).ThenBy(x => x.ClubName)
                .Select(x => (IReadOnlyList<string>)new[]
                {
                    x.Year.ToString(), x.ClubName ?? "-", x.CompetitionName ?? "-",
                    x.Position.ToString(), x.Played.ToString(), x.Won.ToString(), x.Drawn.ToString(),
                    x.Lost.ToString(), (x.GoalsFor - x.GoalsAgainst).ToString(), x.Points.ToString()
                }).ToList();
            ConsoleUIHelper.Table("Clubgeschiedenis",
                new[] { "Jaar", "Club", "Competitie", "Pos", "M", "W", "G", "V", "DS", "Ptn" }, rows);
        }

        public decimal AskPrizeMoneyBudget(string name)
        {
            var culture = CultureInfo.GetCultureInfo("nl-BE");

            Console.Clear();
            Console.WriteLine($"=== Prijzengeld: {name} ===");
            Console.WriteLine("Geef de totale prijzenpot op.");
            Console.WriteLine("Gebruik een komma voor decimalen, bijvoorbeeld 10000,50.");
            Console.WriteLine("Kies 0 om geen prijzengeld uit te keren.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Prijzenpot (€): ");
                var input = Console.ReadLine();

                if (input == null)
                    throw new InvalidOperationException("Console-invoer is afgesloten.");

                if (decimal.TryParse(
                        input,
                        NumberStyles.AllowLeadingWhite |
                        NumberStyles.AllowTrailingWhite |
                        NumberStyles.AllowLeadingSign |
                        NumberStyles.AllowDecimalPoint,
                        culture,
                        out var budget)
                    && budget >= 0
                    && decimal.Round(budget, 2) == budget)
                {
                    return budget;
                }

                Console.WriteLine(
                    "Geef een geldig bedrag vanaf 0 met maximaal twee decimalen.");
            }
        }

        public PrizeMoneyDistribution AskPrizeMoneyDistribution(
            Competition.CompetitionType type)
        {
            var options = type switch
            {
                Competition.CompetitionType.League => new[]
                {
            (
                Distribution: PrizeMoneyDistribution.Equal,
                Description: "Gelijk verdeeld — iedere club krijgt evenveel."
            ),
            (
                Distribution: PrizeMoneyDistribution.Balanced,
                Description: "Evenwichtig — hogere posities krijgen meer."
            ),
            (
                Distribution: PrizeMoneyDistribution.TopHeavy,
                Description: "Topgericht — het grootste deel gaat naar de top."
            )
        },

                Competition.CompetitionType.Cup
                    or Competition.CompetitionType.International => new[]
                {
            (
                Distribution: PrizeMoneyDistribution.WinnerOnly,
                Description: "Alleen winnaar — 100% voor de winnaar."
            ),
            (
                Distribution: PrizeMoneyDistribution.Finalists,
                Description: "Finalisten — winnaar 70%, finalist 30%."
            ),
            (
                Distribution: PrizeMoneyDistribution.SemiFinalists,
                Description:
                    "Vanaf halve finale — winnaar 50%, finalist 25%, " +
                    "verliezende halvefinalisten elk 12,5%."
            )
                },

                _ => throw new ArgumentException(
                    $"Prijzengeld wordt niet ondersteund voor competitietype {type}.",
                    nameof(type))
            };

            Console.Clear();
            Console.WriteLine("=== Verdeling prijzengeld ===");
            Console.WriteLine();

            for (var i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i].Description}");
            }

            Console.WriteLine();

            while (true)
            {
                Console.Write($"Kies een verdeling (1-{options.Length}): ");
                var input = Console.ReadLine();

                if (input == null)
                    throw new InvalidOperationException("Console-invoer is afgesloten.");

                if (int.TryParse(input, out var choice)
                    && choice >= 1
                    && choice <= options.Length)
                {
                    return options[choice - 1].Distribution;
                }

                Console.WriteLine($"Geef een nummer tussen 1 en {options.Length}.");
            }
        }

        public SettingsChoice ShowSettings(PlayerCompetitionSettingsViewModel viewModel)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Settings ===");
                Console.WriteLine();
                Console.WriteLine($"1) Automatisch clubs toevoegen: {(viewModel.AutoAddClub ? "Aan" : "Uit")}");
                Console.WriteLine($"2) Automatisch competitie toevoegen: {(viewModel.AutoAddCompetition ? "Aan" : "Uit")}");
                Console.WriteLine($"3) Minimum clubs in laagste divisie: {viewModel.AutoAddCompetitionThreshold}");
                Console.WriteLine($"4) Automatisch feeder clubs toevoegen: {(viewModel.AutoAddFeederClub ? "Aan" : "Uit")}");
                Console.WriteLine($"5) Prijzengeld aanpassen");
                Console.WriteLine();
                foreach (var comp in viewModel.PrizeMoneyPerCompetition)
                    Console.WriteLine($"{comp.Key}: {comp.Value}");

                Console.WriteLine();
                Console.WriteLine("6) Terug");
                Console.Write("Maak een keuze: ");

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        return SettingsChoice.AutoAddClub;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        return SettingsChoice.AutoAddCompetition;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        return SettingsChoice.AutoAddCompetitionThreshold;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        return SettingsChoice.AutoAddFeederClub;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        return SettingsChoice.PrizeMoneyPerCompetition;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        return SettingsChoice.Back;
                }
            }
        }
    }
}
