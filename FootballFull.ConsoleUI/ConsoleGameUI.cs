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
                ConsoleUIHelper.Header("FOOTBALL FULL");
                Console.WriteLine($"  {dashboard.AssociationName}  |  Seizoen {dashboard.SeasonStartYear}/{dashboard.SeasonStartYear + 1}");
                Console.WriteLine($"  {dashboard.CurrentDate:dddd d MMMM yyyy}");

                ConsoleUIHelper.Section("Verbond");
                ConsoleUIHelper.Value("Saldo", $"{dashboard.Balance:C}");
                ConsoleUIHelper.Value("Reputatie", $"{dashboard.Reputation} - {dashboard.ReputationDescription}");
                ConsoleUIHelper.Value("Clubs", dashboard.ClubCount.ToString());
                ConsoleUIHelper.Value("Competities", dashboard.CompetitionCount.ToString());

                ConsoleUIHelper.Section("Volgende speeldag");
                if (dashboard.NextMatchday.HasValue)
                {
                    ConsoleUIHelper.Value("Datum", dashboard.NextMatchday.Value.ToString("dd/MM/yyyy"));
                    ConsoleUIHelper.Value("Wedstrijden", dashboard.NextMatchCount.ToString());
                }
                else
                {
                    Console.WriteLine("  Geen volgende speeldag gepland.");
                }

                ConsoleUIHelper.Section("Spelen");
                Console.WriteLine("  [1] Verder naar volgende speeldag");

                ConsoleUIHelper.Section("Competities en statistieken");
                Console.WriteLine("  [2] Competities bekijken");
                Console.WriteLine("  [3] Internationale ranglijsten");
                Console.WriteLine("  [4] Competitiegeschiedenis");
                Console.WriteLine("  [5] Clubgeschiedenis");

                ConsoleUIHelper.Section("Beheer");
                Console.WriteLine("  [6] Instellingen");
                Console.WriteLine("  [S] Spel opslaan");
                Console.WriteLine("  [X] Opslaan en afsluiten");
                Console.WriteLine();
                Console.Write("Kies een optie: ");

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.Enter:
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
                    default:
                        ConsoleUIHelper.InvalidChoice();
                        break;
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
            int years = Configuration.InternationalRankingYears;
            var yearRange = Enumerable.Range(currentYear - years + 1, years).ToList();
            var ordered = (rankings ?? Array.Empty<CountryRankingViewModel>())
                .OrderByDescending(x => x.FiveYearCoefficient).ToList();

            var headers = new List<string> { "#", "Land" };
            headers.AddRange(yearRange.Select(y => y.ToString()));
            headers.Add("Totaal");
            var rows = new List<IReadOnlyList<string>>();
            for (int i = 0; i < ordered.Count; i++)
            {
                var r = ordered[i];
                var row = new List<string> { (i + 1).ToString(), r.CountryName };
                row.AddRange(yearRange.Select(y => r.CoefficientPerYear.GetValueOrDefault(y).ToString("F3")));
                row.Add(r.FiveYearCoefficient.ToString("F3"));
                rows.Add(row);
                if (!showDetails) continue;

                var clubs = new List<string> { "", "Deelnemers" };
                clubs.AddRange(yearRange.Select(y => r.ClubsParticipatingPerYear.GetValueOrDefault(y).ToString()));
                clubs.Add("");
                rows.Add(clubs);
                var points = new List<string> { "", "Ruwe punten" };
                points.AddRange(yearRange.Select(y => r.RawPointsPerYear.GetValueOrDefault(y).ToString()));
                points.Add("");
                rows.Add(points);
            }
            ConsoleUIHelper.Table($"Internationale landenranking - {currentYear}",
                headers, rows, waitForUser, showDetails ? 15 : 20);
        }

        public void ShowSeasonEvent(SeasonEventViewModel seasonEvent, bool waitForUser = false)
        {
            ConsoleUIHelper.Header("Seizoensgebeurtenis");
            Console.WriteLine(seasonEvent.Description);
            ConsoleUIHelper.Section("Impact op het verbond");
            ConsoleUIHelper.Value("Saldoverandering", $"{seasonEvent.BalanceChange:+#,##0.##;-#,##0.##;0}");
            ConsoleUIHelper.Value("Reputatieverandering", $"{seasonEvent.ReputationChange:+#;-#;0}");
            if (waitForUser)
                ConsoleUIHelper.Pause();
        }

        public void ShowSeasonFinancialResult(SeasonFinancialResultViewModel result, bool waitForUser = false)
        {
            ConsoleUIHelper.Header($"Financieel seizoensoverzicht - {result.AssociationName}");

            ConsoleUIHelper.Section("Inkomsten");
            ConsoleUIHelper.Value("Bijdragen van clubs", $"{result.ClubIncome:C}");
            ConsoleUIHelper.Value("Reputatie-inkomsten", $"{result.ReputationIncome:C}");
            ConsoleUIHelper.Value("Bonussen", $"{result.BonusIncome:C}");

            ConsoleUIHelper.Section("Uitgaven");
            ConsoleUIHelper.Value("Kosten clubs", $"{result.ClubCosts:C}");
            ConsoleUIHelper.Value("Kosten competities", $"{result.CompetitionCosts:C}");
            ConsoleUIHelper.Value("Organisatiekosten", $"{result.OrganisationCosts:C}");
            ConsoleUIHelper.Value("Subsidies", $"{result.SubsidyCosts:C}");
            ConsoleUIHelper.Value("Prijzengeld", $"{result.PrizeMoneyCosts:C}");

            ConsoleUIHelper.Section("Eindresultaat");
            ConsoleUIHelper.Value("Nettoresultaat", $"{result.NetResult:C}");
            ConsoleUIHelper.Value("Eindsaldo", $"{result.Balance:C}");

            ConsoleUIHelper.Section("Reputatie");
            ConsoleUIHelper.Value("Verandering", $"{result.ReputationChange:+#;-#;0}");
            ConsoleUIHelper.Value("Huidige reputatie", $"{result.Reputation} - {result.ReputationDescription}");

            if (waitForUser)
                ConsoleUIHelper.Pause();
        }

        public Guid AskPlayerToSelectClub(IReadOnlyList<SelectionOptionViewModel> applicants)
        {
            if (applicants == null || applicants.Count == 0)
                throw new ArgumentException("Er zijn geen kandidaat-clubs beschikbaar.", nameof(applicants));

            while (true)
            {
                ConsoleUIHelper.Header("Seizoenseinde - nieuwe club");
                Console.WriteLine("Kies welke kandidaat-club mag toetreden tot de competitie.");
                ConsoleUIHelper.Section("Kandidaten");
                for (var i = 0; i < applicants.Count; i++)
                    Console.WriteLine($"  [{i + 1}] {applicants[i].Name}");

                Console.WriteLine();
                var selected = ConsoleUIHelper.ReadInt("Kies een kandidaat", 1, applicants.Count);
                var candidate = applicants[selected - 1];
                Console.WriteLine();
                Console.WriteLine($"Geselecteerd: {candidate.Name}");
                if (ConsoleUIHelper.Confirm("Deze club toelaten?", defaultAnswer: true))
                    return candidate.Id;
            }
        }

        public bool AskYesNoQuestion(string question, bool defaultAnswer = false)
            => ConsoleUIHelper.Confirm(question, defaultAnswer);

        public string AskForInput(string question, string defaultAnswer = "")
        {
            Console.Write($"{question} ");
            var input = Console.ReadLine();
            if (input == null)
                throw new InvalidOperationException("Console-invoer is afgesloten.");
            return string.IsNullOrWhiteSpace(input) ? defaultAnswer : input.Trim();
        }

        public int AskForClubsToMove(int maximumClubsToMove, int currentClubCount, int minimumClubsToMove = 0)
        {
            if (maximumClubsToMove < minimumClubsToMove)
                throw new InvalidOperationException("Er zijn onvoldoende clubs om twee geldige divisies te maken.");

            while (true)
            {
                ConsoleUIHelper.Header("Seizoenseinde - lagere divisie");
                Console.WriteLine("De laagst geklasseerde clubs verhuizen naar de nieuwe divisie.");
                ConsoleUIHelper.Section("Verdeling");
                ConsoleUIHelper.Value("Huidige clubs", currentClubCount.ToString());
                ConsoleUIHelper.Value("Minimaal te verplaatsen", minimumClubsToMove.ToString());
                ConsoleUIHelper.Value("Maximaal te verplaatsen", maximumClubsToMove.ToString());
                Console.WriteLine();

                int count = ConsoleUIHelper.ReadInt("Aantal clubs verplaatsen", minimumClubsToMove, maximumClubsToMove);
                ConsoleUIHelper.Section("Voorbeeld na herstructurering");
                ConsoleUIHelper.Value("Bestaande divisie", (currentClubCount - count).ToString());
                ConsoleUIHelper.Value("Nieuwe divisie", count.ToString());
                if (ConsoleUIHelper.Confirm("Deze verdeling bevestigen?", defaultAnswer: true))
                    return count;
            }
        }

        public bool AskToCreateLowerDivision()
        {
            while (true)
            {
                ConsoleUIHelper.Header("Seizoenseinde - competitie-uitbreiding");
                Console.WriteLine("Je kunt een club toelaten of de competitiestructuur uitbreiden.");
                ConsoleUIHelper.Section("Keuzes");
                Console.WriteLine("  [1] Een nieuwe club toelaten");
                Console.WriteLine("  [2] Een lagere divisie oprichten");
                Console.WriteLine();
                var choice = ConsoleUIHelper.ReadInt("Maak een keuze", 1, 2);
                if (choice == 1) return false;
                if (ConsoleUIHelper.Confirm("Doorgaan met het oprichten van een lagere divisie?", true))
                    return true;
            }
        }

        public IList<string> AskStarterClubNames(int numberOfClubs)
        {
            var names = new List<string>();
            ConsoleUIHelper.Header("Nieuwe starterclubs");
            for (int i = 1; i <= numberOfClubs; i++)
            {
                while (true)
                {
                    Console.Write($"Club {i}/{numberOfClubs}: ");
                    var name = Console.ReadLine()?.Trim();
                    if (name == null) throw new InvalidOperationException("Console-invoer is afgesloten.");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("De clubnaam mag niet leeg zijn.");
                        continue;
                    }
                    if (names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Deze clubnaam is al ingevoerd.");
                        continue;
                    }
                    names.Add(name);
                    break;
                }
            }
            return names;
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
            // Zonder pauze tonen we alle clubs, zodat er geen informatie verdwijnt.
            // Bij interactieve weergave groeperen we de clubs per vijf.
            const int pageSize = 5;
            int count = clubs?.Count ?? 0;
            int pageCount = Math.Max(1, (count + pageSize - 1) / pageSize);
            int page = 0;

            while (true)
            {
                ConsoleUIHelper.Header("Clubfinanciën en ontwikkeling");
                Console.WriteLine("Een deel van de winst wordt gereserveerd voor ontwikkeling; investeren kost geld.");

                if (count == 0)
                    Console.WriteLine("\nGeen clubontwikkelingen beschikbaar.");

                var visibleClubs = waitForUser
                    ? clubs.Skip(page * pageSize).Take(pageSize)
                    : clubs;

                foreach (var club in visibleClubs ?? Enumerable.Empty<ClubDevelopmentViewModel>())
                {
                    ConsoleUIHelper.Section(club.ClubName);
                    ConsoleUIHelper.Value("Sterkte", $"{club.StrengthBefore} → {club.StrengthAfter}");
                    ConsoleUIHelper.Value("Nettoresultaat", $"{club.NetResult:N0}");
                    ConsoleUIHelper.Value("Investering", $"{club.Investment:N0}");
                    ConsoleUIHelper.Value("Ontvangen prijzengeld", $"{club.PrizeMoneyReceived:N0}");
                    ConsoleUIHelper.Value("Ontwikkelingsbudget", $"{club.DevelopmentBudget:N0}");
                    ConsoleUIHelper.Value("Saldo", $"{club.Balance:N0}");
                    if (club.CreateFeederClubCost > 0)
                        ConsoleUIHelper.Value("Kosten B-ploeg", $"{club.CreateFeederClubCost:N0}");
                    ConsoleUIHelper.Value("Toelichting", club.Reason);
                }

                if (!waitForUser) return;

                Console.WriteLine();
                Console.WriteLine($"Pagina {page + 1}/{pageCount} · [V] Volgende · [P] Vorige · [Enter] Verder");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter || key == ConsoleKey.Escape) return;
                if (key == ConsoleKey.V || key == ConsoleKey.RightArrow)
                    page = Math.Min(pageCount - 1, page + 1);
                if (key == ConsoleKey.P || key == ConsoleKey.LeftArrow)
                    page = Math.Max(0, page - 1);
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
            ConsoleUIHelper.Header($"Prijzengeld - {name}");
            Console.WriteLine("Bepaal de totale prijzenpot voor deze competitie.");
            Console.WriteLine("Gebruik een komma voor decimalen. 0 betekent geen prijzengeld.");
            ConsoleUIHelper.Section("Budget");

            while (true)
            {
                Console.Write("Prijzenpot (€): ");
                var input = Console.ReadLine();
                if (input == null) throw new InvalidOperationException("Console-invoer is afgesloten.");
                if (!decimal.TryParse(input, NumberStyles.AllowLeadingWhite |
                    NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint,
                    culture, out var amount) || amount < 0 || decimal.Round(amount, 2) != amount)
                {
                    Console.WriteLine("Geef een bedrag vanaf 0 met maximaal twee decimalen, bijvoorbeeld 10000,50.");
                    continue;
                }

                ConsoleUIHelper.Section("Controle");
                ConsoleUIHelper.Value("Competitie", name);
                ConsoleUIHelper.Value("Prijzenpot", amount.ToString("C", culture));
                if (ConsoleUIHelper.Confirm("Deze prijzenpot bevestigen?", true))
                    return amount;
                Console.WriteLine("Voer een ander budget in.");
            }
        }

        public PrizeMoneyDistribution AskPrizeMoneyDistribution(Competition.CompetitionType type)
        {
            var options = type switch
            {
                Competition.CompetitionType.League => new[]
                {
                    (Distribution: PrizeMoneyDistribution.Equal, Description: "Gelijk - iedere club krijgt evenveel."),
                    (Distribution: PrizeMoneyDistribution.Balanced, Description: "Evenwichtig - hogere posities krijgen meer."),
                    (Distribution: PrizeMoneyDistribution.TopHeavy, Description: "Topgericht - grootste deel voor de top.")
                },
                Competition.CompetitionType.Cup or Competition.CompetitionType.International => new[]
                {
                    (Distribution: PrizeMoneyDistribution.WinnerOnly, Description: "Alleen winnaar - winnaar ontvangt 100%."),
                    (Distribution: PrizeMoneyDistribution.Finalists, Description: "Finalisten - winnaar 70%, finalist 30%."),
                    (Distribution: PrizeMoneyDistribution.SemiFinalists, Description: "Halve finales - winnaar 50%, finalist 25%, beide halvefinalisten 12,5%.")
                },
                _ => throw new ArgumentException($"Prijzengeld niet ondersteund voor {type}.", nameof(type))
            };

            while (true)
            {
                ConsoleUIHelper.Header("Seizoenseinde - prijzengeldverdeling");
                Console.WriteLine($"Competitietype: {type}");
                ConsoleUIHelper.Section("Verdelingsmodellen");
                for (int i = 0; i < options.Length; i++)
                    Console.WriteLine($"  [{i + 1}] {options[i].Description}");
                Console.WriteLine();
                int index = ConsoleUIHelper.ReadInt("Kies een verdeling", 1, options.Length) - 1;
                ConsoleUIHelper.Section("Geselecteerde verdeling");
                Console.WriteLine(options[index].Description);
                if (ConsoleUIHelper.Confirm("Deze verdeling bevestigen?", true))
                    return options[index].Distribution;
            }
        }

        public SettingsChoice ShowSettings(PlayerCompetitionSettingsViewModel viewModel)
        {
            while (true)
            {
                ConsoleUIHelper.Header("Instellingen");
                ConsoleUIHelper.Section("Automatisering");
                Console.WriteLine($"  [1] Clubs automatisch toevoegen       {ConsoleUIHelper.Toggle(viewModel.AutoAddClub)}");
                Console.WriteLine($"  [2] Competities automatisch toevoegen {ConsoleUIHelper.Toggle(viewModel.AutoAddCompetition)}");
                Console.WriteLine($"  [3] Minimum clubs laagste divisie      {viewModel.AutoAddCompetitionThreshold}");
                Console.WriteLine($"  [4] B-ploegen automatisch toevoegen    {ConsoleUIHelper.Toggle(viewModel.AutoAddFeederClub)}");

                ConsoleUIHelper.Section("Financiën");
                Console.WriteLine("  [5] Prijzengeld aanpassen");
                if (viewModel.PrizeMoneyPerCompetition != null && viewModel.PrizeMoneyPerCompetition.Any())
                {
                    foreach (var competition in viewModel.PrizeMoneyPerCompetition)
                        ConsoleUIHelper.Value(competition.Key.ToString(), competition.Value.ToString());
                }
                else
                {
                    Console.WriteLine("  Nog geen prijzengeld ingesteld.");
                }

                ConsoleUIHelper.Section("Navigatie");
                Console.WriteLine("  [6] Terug naar hoofdmenu");
                Console.WriteLine();
                Console.Write("Kies een instelling: ");

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
                    case ConsoleKey.Escape:
                    case ConsoleKey.Backspace:
                        return SettingsChoice.Back;
                    default:
                        ConsoleUIHelper.InvalidChoice();
                        break;
                }
            }
        }
    }
}
