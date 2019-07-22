using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            _commands.Add(new Move()); 

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
                    var keys = command.commandOptions.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        commandList += " ["+keys[i].ToString()+"] ";
                    }
                }

                return new ConsoleOutput("Available commands are "+ commandList, ConsoleOutput.OutputType.Log);
            }

        }
        class Move : Command
        {
             public Move()
            {
                queryIdentity = "move";
                commandOptions.Add("transform",new CommandOption<Transform>());
                commandOptions.Add("position", new CommandOption<Vector3>());
            }

            public override ConsoleOutput Logic()
            {
                var trans = (Transform)((CommandOption)(commandOptions["transform"] as CommandOption<Transform>)).optionParameter;
                var vec = (commandOptions["position"] as CommandOption<Vector3>).optionParameter;

                Debug.Log("transported");
                if (trans == null)
                {
                    return new ConsoleOutput("Transform couldn't found.", ConsoleOutput.OutputType.Log);

                }
                if (vec == null)
                {
                    return new ConsoleOutput("Vector couldn't found.", ConsoleOutput.OutputType.Log);

                }
                trans.position = vec;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + vec.ToString() , ConsoleOutput.OutputType.Log);
            }

        }
    }
}