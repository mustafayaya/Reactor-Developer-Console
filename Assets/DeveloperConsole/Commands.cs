using System;
using System.Collections;
using System.Collections.Generic;

namespace Console
{
    public class Commands
    {
        private static Commands instance = null;


        private List<Command> _commands = new List<Command>();
        
        public Commands()
        {
            RegisterCommands();
        }
        public static Commands Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Commands();
                }
                return instance;
            }
        }
    
         public List<Command> GetCommands()
        {

            return _commands;
        }
        public void RegisterCommands()
        {
            _commands.Add(new Help());

            foreach (Command c in _commands)
            {
                if (String.IsNullOrEmpty(c.queryIdentity))
                {
                    var message = "Command " + c + "("+c.GetHashCode()+") doesn't has a query identity. It will be ignored." ;
                    Console.DeveloperConsole.WriteWarning(message);
                    _commands.Remove(c);
                }
            }
            
        }

        class Help : Command
        {
            public Help()
            {
                queryIdentity = "help";
            }

            public override ConsoleOutput Logic()
            {
                string commandList = "\n";

                foreach (Command command in Commands.Instance.GetCommands())
                {
                    commandList = commandList + "\n -" + command.queryIdentity;
                }

                return new ConsoleOutput("Available commands are "+ commandList, ConsoleOutput.OutputType.Log);
            }

        }
      
    }
}