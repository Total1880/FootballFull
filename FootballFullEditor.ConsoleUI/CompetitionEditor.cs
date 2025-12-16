using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFullEditor.ConsoleUI
{
    public class CompetitionEditor
    {
        private readonly ICompetitionService _competitionService;
        private readonly ICountryService _countryService;
        private readonly IClubService _clubService;
        private readonly IClubPerCompetitionService _clubCompetitionService;

        public CompetitionEditor(
            ICompetitionService competitionService,
            ICountryService countryService,
            IClubService clubService,
            IClubPerCompetitionService clubCompetitionService)
        {
            _competitionService = competitionService;
            _countryService = countryService;
            _clubService = clubService;
            _clubCompetitionService = clubCompetitionService;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("COMPETITIONS");
                Console.WriteLine("------------");
                ShowCompetitions();

                Console.WriteLine();
                Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit  |  [M]anage clubs  |  [B]ack");

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.A:
                        AddCompetition();
                        break;
                    case ConsoleKey.D:
                        DeleteCompetition();
                        break;
                    case ConsoleKey.E:
                        EditCompetition();
                        break;
                    case ConsoleKey.M:
                        ManageClubsForCompetition();
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void ShowCompetitions()
        {
            var comps = _competitionService.GetCompetitions();
            var countries = _countryService.GetCountries().ToDictionary(c => c.Id, c => c.Name);

            if (comps.Count == 0)
            {
                Console.WriteLine("No competitions found.");
                return;
            }

            for (int i = 0; i < comps.Count; i++)
            {
                var c = comps[i];
                var countryName = countries.ContainsKey(c.CountryId) ? countries[c.CountryId] : "Unknown";

                Console.WriteLine($"{i + 1}. {c.Name} (Tier {c.Tier}, {c.Type}, CountryId: {c.CountryId})");
            }
        }

        private Guid SelectCountry()
        {
            var list = _countryService.GetCountries().ToList();
            Console.Clear();
            Console.WriteLine("Select a country:");

            if (list.Count == 0)
            {
                Console.WriteLine("No countries available. Press any key...");
                Console.ReadKey();
                return Guid.Empty;
            }

            for (int i = 0; i < list.Count; i++)
                Console.WriteLine($"{i + 1}. {list[i].Name}");

            Console.Write("Choose number (or ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return Guid.Empty;

            if (!int.TryParse(input, out var index))
                return Guid.Empty;

            if (index < 1 || index > list.Count)
                return Guid.Empty;

            return list[index - 1].Id;
        }

        private void AddCompetition()
        {
            Console.Clear();
            Console.WriteLine("Adding new competition...");

            Console.Write("Name: ");
            var name = Console.ReadLine();

            Console.Write("Tier: ");
            int.TryParse(Console.ReadLine(), out int tier);

            var countryId = SelectCountry();
            if (countryId == Guid.Empty)
            {
                Console.WriteLine("Cancelled. Press any key...");
                Console.ReadKey();
                return;
            }

            // Nieuw: type kiezen
            var type = SelectCompetitionType();

            _competitionService.Add(new Competition
            {
                Name = name,
                Tier = tier,
                CountryId = countryId,
                Type = type
            });

            Console.WriteLine("Competition added. Press any key...");
            Console.ReadKey();
        }

        private void DeleteCompetition()
        {
            Console.Clear();
            Console.WriteLine("Deleting competition...");
            ShowCompetitions();

            Console.Write("Enter number: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var index))
                return;

            var comps = _competitionService.GetCompetitions();
            if (index < 1 || index > comps.Count)
                return;

            _competitionService.Delete(comps[index - 1].Id);

            Console.WriteLine("Deleted. Press any key...");
            Console.ReadKey();
        }

        private void EditCompetition()
        {
            Console.Clear();
            Console.WriteLine("Editing competition...");
            ShowCompetitions();

            Console.Write("Enter number: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var index))
                return;

            var comps = _competitionService.GetCompetitions();
            if (index < 1 || index > comps.Count)
                return;

            var comp = comps[index - 1];

            Console.WriteLine($"Current name: {comp.Name}");
            Console.Write("New name (leave empty to keep): ");
            var newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                comp.Name = newName;

            Console.WriteLine($"Current tier: {comp.Tier}");
            Console.Write("New tier (leave empty to keep): ");
            var tierInput = Console.ReadLine();
            if (int.TryParse(tierInput, out int newTier))
                comp.Tier = newTier;

            Console.WriteLine();
            Console.WriteLine($"Current country: {comp.CountryId}");
            Console.WriteLine("Change country? Y/N");
            if (Console.ReadKey(true).Key == ConsoleKey.Y)
            {
                var newCountryId = SelectCountry();
                if (newCountryId != Guid.Empty)
                    comp.CountryId = newCountryId;
            }

            // Nieuw: type aanpassen
            Console.WriteLine();
            comp.Type = SelectCompetitionType(comp.Type, allowEmpty: true);

            _competitionService.Update(comp);

            Console.WriteLine("Updated. Press any key...");
            Console.ReadKey();
        }

        private void ManageClubsForCompetition()
        {
            Console.Clear();
            Console.WriteLine("Manage clubs in competition");
            ShowCompetitions();

            Console.Write("Enter number of competition (or ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
                return;

            var comps = _competitionService.GetCompetitions();
            if (index < 1 || index > comps.Count)
                return;

            var competition = comps[index - 1];

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Clubs in {competition.Name}");
                Console.WriteLine(new string('-', 30));

                var clubsInComp = _clubCompetitionService
                    .GetClubsForCompetition(competition.Id)
                    .ToList();

                if (clubsInComp.Count == 0)
                {
                    Console.WriteLine("No clubs in this competition.");
                }
                else
                {
                    for (int i = 0; i < clubsInComp.Count; i++)
                    {
                        var c = clubsInComp[i];
                        Console.WriteLine($"{i + 1}. {c.Name} (Strength {c.Strength})");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("[A]dd club  |  [R]emove club  |  [B]ack");
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.A:
                        AddClubToCompetition(competition);
                        break;
                    case ConsoleKey.R:
                        RemoveClubFromCompetition(competition, clubsInComp);
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void AddClubToCompetition(Competition competition)
        {
            Console.Clear();
            Console.WriteLine($"Add club to {competition.Name}");

            var allClubs = _clubService.GetClubs().Where(_ => _.CountryId == competition.CountryId).ToList();
            var existing = _clubCompetitionService
                .GetAllClubPerCompetitions()
                .Select(c => c.ClubId)
                .ToHashSet();

            var availableClubs = allClubs
                .Where(c => !existing.Contains(c.Id))
                .ToList();

            if (availableClubs.Count == 0)
            {
                Console.WriteLine("No available clubs to add.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < availableClubs.Count; i++)
            {
                var c = availableClubs[i];
                Console.WriteLine($"{i + 1}. {c.Name} (Strength {c.Strength})");
            }

            Console.Write("Enter number (or ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
                return;

            if (index < 1 || index > availableClubs.Count)
                return;

            var club = availableClubs[index - 1];

            _clubCompetitionService.AddClubToCompetition(club.Id, competition.Id);

            Console.WriteLine($"Club '{club.Name}' added to {competition.Name}. Press any key...");
            Console.ReadKey();
        }

        private void RemoveClubFromCompetition(Competition competition, List<Club> clubsInComp)
        {
            if (clubsInComp == null || clubsInComp.Count == 0)
            {
                Console.WriteLine("No clubs to remove. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Remove club from {competition.Name}");

            for (int i = 0; i < clubsInComp.Count; i++)
            {
                var c = clubsInComp[i];
                Console.WriteLine($"{i + 1}. {c.Name} (Strength {c.Strength})");
            }

            Console.Write("Enter number (or ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
                return;

            if (index < 1 || index > clubsInComp.Count)
                return;

            var club = clubsInComp[index - 1];

            _clubCompetitionService.RemoveClubFromCompetition(club.Id, competition.Id);

            Console.WriteLine($"Club '{club.Name}' removed from {competition.Name}. Press any key...");
            Console.ReadKey();
        }

        private Competition.CompetitionType SelectCompetitionType(
    Competition.CompetitionType? current = null,
    bool allowEmpty = false)
        {
            while (true)
            {
                Console.WriteLine();
                if (current.HasValue)
                    Console.WriteLine($"Current type: {current.Value}");

                Console.WriteLine("Select competition type:");
                Console.WriteLine("1) League");
                Console.WriteLine("2) Cup");
                Console.WriteLine("3) International");

                if (allowEmpty)
                    Console.WriteLine("Leave empty to keep current value.");

                Console.Write("Choice: ");
                var input = Console.ReadLine();

                if (allowEmpty && string.IsNullOrWhiteSpace(input) && current.HasValue)
                    return current.Value;

                if (int.TryParse(input, out var choice))
                {
                    switch (choice)
                    {
                        case 1: return Competition.CompetitionType.League;
                        case 2: return Competition.CompetitionType.Cup;
                        case 3: return Competition.CompetitionType.International;
                    }
                }

                Console.WriteLine("Invalid choice, try again...");
            }
        }

    }
}
