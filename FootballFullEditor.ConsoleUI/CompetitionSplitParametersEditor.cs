using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFullEditor.ConsoleUI
{
    public class CompetitionSplitParametersEditor
    {
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
            }
        }

        private void ShowCompetitionSplitParameters()
        {
            throw new NotImplementedException();
        }
    }
}
