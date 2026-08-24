using FootballFull.Models;
using FootballFull.Services;
using FootballFull.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFullEditor.ConsoleUI
{
    public class CompetitionSplitParametersEditor
    {
        private readonly ICompetitionSplitParametersService _competitionSplitParametersService;

        public CompetitionSplitParametersEditor(ICompetitionSplitParametersService competitionSplitParametersService)
        {
            _competitionSplitParametersService = competitionSplitParametersService;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("COMPETITION Split Parameters");
                Console.WriteLine("------------");
                ShowCompetitionSplitParameters();

                Console.WriteLine();
                Console.WriteLine("[A]dd  |  [D]elete  |  [E]dit  |  [B]ack");

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.A:
                        AddSplitParameter();
                        break;
                    case ConsoleKey.D:
                        DeleteSplitParameter(); 
                        break;
                    case ConsoleKey.E:
                        EditSplitParameter();
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void EditSplitParameter()
        {
            Console.Clear();
            Console.WriteLine("Editing competition split parameters...");
            ShowCompetitionSplitParameters();

            Console.Write("Enter number: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var index))
                return;

            var cspAll = _competitionSplitParametersService.GetCompetitionSplitParameters();
            if (index < 1 || index > cspAll.Count)
                return;

            var csp = cspAll[index - 1];
            Console.WriteLine($"Editing: {csp.Name}");
            Console.Write("Enter new name: ");
            var newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                csp.Name = newName;
                _competitionSplitParametersService.Update(csp);
            }
        }

        private void DeleteSplitParameter()
        {
            Console.Clear();
            Console.WriteLine("Delete competition split parameters...");
            ShowCompetitionSplitParameters();

            Console.Write("Enter number: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var index))
                return;

            var cspAll = _competitionSplitParametersService.GetCompetitionSplitParameters();
            if (index < 1 || index > cspAll.Count)
                return;

            var csp = cspAll[index - 1];
            Console.WriteLine($"Deleting: {csp.Name}");
            Console.Write("Are you sure? (Y/N): ");
            var confirm = Console.ReadKey(true).Key;
            if (confirm == ConsoleKey.Y)
            {
                _competitionSplitParametersService.Delete(csp.Id);
            }
        }

        private void AddSplitParameter()
        {
            Console.Clear();
            Console.WriteLine("Add competition split parameters...");

            Console.Write("Enter name: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) {
                var csp = new CompetitionSplitParameters { Name = name };
                _competitionSplitParametersService.Add(csp);
            }
        }

        private void ShowCompetitionSplitParameters()
        {
            var counter = 1;
            _competitionSplitParametersService.GetCompetitionSplitParameters().ToList().ForEach(csp =>
            {
                Console.WriteLine($"{counter++}. Name: {csp.Name}");
            });
        }
    }
}
