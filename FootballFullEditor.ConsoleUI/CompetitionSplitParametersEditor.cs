using FootballFull.Models;
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
                        AddCompetitionSplitParameters();
                        break;
                    case ConsoleKey.D:
                        DeleteCompetitionSplitParameters();
                        break;
                    case ConsoleKey.E:
                        EditCompetitionSplitParameters();  
                        break;
                    case ConsoleKey.B:
                        return;
                }
            }
        }

        private void EditCompetitionSplitParameters()
        {
            throw new NotImplementedException();
        }

        private void DeleteCompetitionSplitParameters()
        {
            throw new NotImplementedException();
        }

        private void AddCompetitionSplitParameters()
        {
            Console.Clear();
            Console.WriteLine("Adding new competition split parameter...");

            Console.Write("Name: ");
            var name = Console.ReadLine();

            _competitionSplitParametersService.Add(new CompetitionSplitParameters { Name = name });
        }

        private void ShowCompetitionSplitParameters()
        {
            var competitionSplitParameters = _competitionSplitParametersService.GetCompetitionSplitParameters();
            foreach (var c in competitionSplitParameters)
            {
                Console.WriteLine($"ID: {c.Id}, Name: {c.Name}");
            }
        }
    }
}
