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
            _commands.Add(new Rotate());

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

                //Debug.Log("transported");
                if (trans == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong(trans), ConsoleOutput.OutputType.Log);

                }
                if (vec == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong(vec), ConsoleOutput.OutputType.Log);

                }
                trans.position = vec;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + vec.ToString() , ConsoleOutput.OutputType.Log);
            }

        }

        class Rotate : Command
        {
            public Rotate()
            {
                queryIdentity = "rotate";
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("rotation", new CommandOption<Quaternion>());
            }

            public override ConsoleOutput Logic()
            {
                var trans = (Transform)((CommandOption)(commandOptions["transform"] as CommandOption<Transform>)).optionParameter;
                var quaternion = (commandOptions["rotation"] as CommandOption<Quaternion>).optionParameter;

                if (trans == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong(trans), ConsoleOutput.OutputType.Log);

                }
                if (quaternion == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong(quaternion), ConsoleOutput.OutputType.Log);

                }
                trans.rotation = quaternion;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + quaternion.ToString(), ConsoleOutput.OutputType.Log);
            }

        }
    }
}