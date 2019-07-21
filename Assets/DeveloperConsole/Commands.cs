using System.Collections;
using System.Collections.Generic;

namespace Console
{
    public class Commands
    {
        private List<Command> _commands = new List<Command>();
        
        public Commands()
        {

        }

        public List<Command> GetCommands()
        {
            return _commands;
        }
        public void RegisterCommands()
        {
            _commands.Add(new Help());
            
        }

        class Help : Command
        {
            public Help()
            {
                queryIdentity = "help";
            }

            public override ConsoleOutput Logic()
            {
                return new ConsoleOutput("Available commands are...", ConsoleOutput.OutputType.Log);
            }

        }


    }
}