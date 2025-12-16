using FootballFull.Models;
using FootballFull.Services.Interfaces;

namespace FootballFullEditor.ConsoleUI
{
    public class CountryEditor
    {
        private readonly ICountryService _countryService;

        public CountryEditor(ICountryService countryService)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("COUNTRIES");
                Console.WriteLine("---------");
                ShowCountries();

                Console.WriteLine();
                Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit  |  [B]ack");

                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.A:
                        AddCountry();
                        break;
                    case ConsoleKey.D:
                        DeleteCountry();
                        break;
                    case ConsoleKey.E:
                        EditCountryInternal();
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void ShowCountries()
        {
            var countries = _countryService.GetCountries();

            if (countries == null || countries.Count == 0)
            {
                Console.WriteLine("No countries found.");
                return;
            }

            for (int i = 0; i < countries.Count; i++)
            {
                var c = countries[i];
                Console.WriteLine($"{i + 1}. {c.Name} ({c.Id})");
            }
        }

        private void AddCountry()
        {
            Console.Clear();
            Console.WriteLine("Adding a new country...");
            Console.Write("Name: ");
            var name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            _countryService.Add(new Country
            {
                // Id mag leeg zijn, repository zet een Guid
                Name = name.Trim()
            });

            Console.WriteLine("Country added. Press any key to continue...");
            Console.ReadKey();
        }

        private void DeleteCountry()
        {
            Console.Clear();
            Console.WriteLine("Deleting a country...");
            ShowCountries();

            Console.WriteLine();
            Console.Write("Enter the number of the country to delete (or press ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
            {
                Console.WriteLine("Invalid number. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var list = _countryService.GetCountries().ToList();
            if (index < 1 || index > list.Count)
            {
                Console.WriteLine("Number out of range. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var countryToDelete = list[index - 1];
            _countryService.Delete(countryToDelete.Id);

            Console.WriteLine($"Country '{countryToDelete.Name}' deleted. Press any key to continue...");
            Console.ReadKey();
        }

        private void EditCountryInternal()
        {
            Console.Clear();
            Console.WriteLine("Editing a country...");
            ShowCountries();

            Console.WriteLine();
            Console.Write("Enter the number of the country to edit (or press ENTER to cancel): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input, out var index))
            {
                Console.WriteLine("Invalid number. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var list = _countryService.GetCountries().ToList();
            if (index < 1 || index > list.Count)
            {
                Console.WriteLine("Number out of range. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var countryToEdit = list[index - 1];

            Console.WriteLine($"Current name: {countryToEdit.Name}");
            Console.Write("New name (leave empty to cancel): ");
            var newName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Edit cancelled. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            countryToEdit.Name = newName.Trim();
            _countryService.Update(countryToEdit);

            Console.WriteLine("Country updated. Press any key to continue...");
            Console.ReadKey();
        }
    }
}
