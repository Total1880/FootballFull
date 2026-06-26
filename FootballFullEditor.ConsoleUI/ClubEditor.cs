using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFullEditor.ConsoleUI
{
    public class ClubEditor
    {
        private readonly IClubService _clubService;
        private readonly ICountryService _countryService;

        public ClubEditor(IClubService clubService, ICountryService countryService)
        {
            _clubService = clubService ?? throw new ArgumentNullException(nameof(clubService));
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("CLUBS");
                Console.WriteLine("-----");
                ShowClubs();

                Console.WriteLine();
                Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit  |  [B]ack");

                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.A:
                        AddClub();
                        break;
                    case ConsoleKey.D:
                        DeleteClub();
                        break;
                    case ConsoleKey.E:
                        EditClubInternal();
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void ShowClubs()
        {
            var clubs = _clubService.GetClubs();

            if (clubs == null || clubs.Count == 0)
            {
                Console.WriteLine("No clubs found.");
                return;
            }

            var countries = _countryService.GetCountries().ToDictionary(c => c.Id, c => c.Name);

            for (int i = 0; i < clubs.Count; i++)
            {
                var c = clubs[i];
                countries.TryGetValue(c.CountryId, out var countryName);
                Console.WriteLine($"{i + 1}. {c.Name} - Strength: {c.Strength} - Country: {countryName ?? "Unknown"} ({c.Id})");
            }
        }

        private void AddClub()
        {
            Console.Clear();
            Console.WriteLine("Adding a new club...");
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? string.Empty;

            Console.Write("Strength: ");
            var strengthInput = Console.ReadLine();
            if (!int.TryParse(strengthInput, out var strength))
                strength = 0;

            var countryId = SelectCountry();
            if (countryId == Guid.Empty)
            {
                Console.WriteLine("No country selected. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Has feeder club? (y/n)");
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Y)
                _clubService.Add(new Club
                {
                    Id = Guid.NewGuid(),
                    Name = name.Trim() + " B",
                    Strength = strength - 20,
                    CountryId = countryId
                });

            _clubService.Add(new Club
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Strength = strength,
                CountryId = countryId
            });

            Console.WriteLine("Club added. Press any key to continue...");
            Console.ReadKey();
        }

        private void DeleteClub()
        {
            Console.Clear();
            Console.WriteLine("Deleting a club...");
            ShowClubs();

            Console.WriteLine();
            Console.Write("Enter the number of the club to delete (or press ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
            {
                Console.WriteLine("Invalid number. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var list = _clubService.GetClubs().ToList();
            if (index < 1 || index > list.Count)
            {
                Console.WriteLine("Number out of range. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var clubToDelete = list[index - 1];
            if(clubToDelete.FeederClubId != null)
            {
                var feederClub = _clubService.GetClubById((Guid)clubToDelete.FeederClubId);
                _clubService.Delete(feederClub.Id);
                Console.WriteLine($"Feeder Club '{feederClub.Name}' deleted.");
            }
            _clubService.Delete(clubToDelete.Id);

            if(_clubService.FindParentClub(clubToDelete.Id) != null) { }

            Console.WriteLine($"Club '{clubToDelete.Name}' deleted. Press any key to continue...");
            Console.ReadKey();
        }

        private void EditClubInternal()
        {
            Console.Clear();
            Console.WriteLine("Editing a club...");
            ShowClubs();

            Console.WriteLine();
            Console.Write("Enter the number of the club to edit (or press ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
            {
                Console.WriteLine("Invalid number. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var list = _clubService.GetClubs().ToList();
            if (index < 1 || index > list.Count)
            {
                Console.WriteLine("Number out of range. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var club = list[index - 1];
            Club feederClub = club.FeederClubId == null || club.FeederClubId == Guid.Empty ? new Club { Name = club.Name + " B" } : _clubService.GetClubById((Guid)club.FeederClubId);
            if (feederClub.CountryId == Guid.Empty) feederClub.CountryId = club.CountryId;

            Console.WriteLine($"Current name: {club.Name}");
            Console.Write("New name (leave empty to keep): ");
            var newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                club.Name = newName.Trim();
                feederClub.Name = club.Name + " B";
            }

            Console.WriteLine($"Current strength: {club.Strength}");
            Console.Write("New strength (leave empty to keep): ");
            var newStrengthInput = Console.ReadLine();
            if (int.TryParse(newStrengthInput, out var newStrength))
            {
                club.Strength = newStrength;
                feederClub.Strength = newStrength - 20;
            }

            Console.WriteLine("Change country? [Y/N]");
            var changeCountryKey = Console.ReadKey(intercept: true).Key;
            if (changeCountryKey == ConsoleKey.Y)
            {
                var newCountryId = SelectCountry();
                if (newCountryId != Guid.Empty)
                {
                    club.CountryId = newCountryId;
                    feederClub.CountryId = newCountryId;
                }
            }
            Console.WriteLine("Has feeder club? [Y/N]");
            var feederClubKey = Console.ReadKey(intercept: true).Key;
            if (feederClubKey == ConsoleKey.Y)
            {
                if (club.FeederClubId == null || club.FeederClubId == Guid.Empty)
                {
                    feederClub.Id = new Guid();
                    club.FeederClubId = feederClub.Id;
                    _clubService.Add(feederClub);
                    Console.WriteLine("Feeder Club added.");
                }
                else
                {
                    _clubService.Update(feederClub);
                    Console.WriteLine("Feeder Club updated.");
                }
            }
            else if (feederClubKey == ConsoleKey.N)
            {
                if (club.FeederClubId != null && club.FeederClubId != Guid.Empty)
                {
                    _clubService.Delete((Guid)club.FeederClubId);
                    Console.WriteLine("Feeder Club Deleted.");
                }
            }
            else if(feederClub.Id != Guid.Empty) 
            {
                _clubService.Update(feederClub);
                Console.WriteLine("Feeder Club updated.");
            }
            _clubService.Update(club);

            Console.WriteLine("\nClub updated. Press any key to continue...");
            Console.ReadKey();
        }

        private Guid SelectCountry()
        {
            Console.Clear();
            Console.WriteLine("Select a country:");
            var countries = _countryService.GetCountries().ToList();

            if (countries.Count == 0)
            {
                Console.WriteLine("No countries available. Press any key to continue...");
                Console.ReadKey();
                return Guid.Empty;
            }

            for (int i = 0; i < countries.Count; i++)
            {
                var c = countries[i];
                Console.WriteLine($"{i + 1}. {c.Name} ({c.Id})");
            }

            Console.WriteLine();
            Console.Write("Enter the number of the country (or press ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return Guid.Empty;

            if (!int.TryParse(input, out var index))
                return Guid.Empty;

            if (index < 1 || index > countries.Count)
                return Guid.Empty;

            return countries[index - 1].Id;
        }
    }
}
