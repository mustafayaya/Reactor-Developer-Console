using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Compilation;
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
            var commands = Utility.GetTypesWithCommandAttribute(System.AppDomain.CurrentDomain.GetAssemblies());

            foreach (Command command in commands)
            {
                _commands.Add(command);
            }


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


        [ConsoleCommand]
        class Help : Command
        {
            public Help()
            {
                queryIdentity = "help";
                description = "List all available commands";

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

                    commandList += "  --" + command.description;

                }

                return new ConsoleOutput("Available commands are "+ commandList, ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand]
        class Move : Command
        {
             public Move()
            {
                queryIdentity = "move";
                commandOptions.Add("transform",new CommandOption<Transform>());
                commandOptions.Add("position", new CommandOption<Vector3>());
                description = "Translate a game object's transform to a world point";
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

        [ConsoleCommand]
        class Rotate : Command
        {
            public Rotate()
            {
                queryIdentity = "rotate";
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("rotation", new CommandOption<Quaternion>());
                description = "Rotate a game object";

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

        [ConsoleCommand]
        class Sphere : Command
        {
            public Sphere()
            {
                queryIdentity = "sphere";
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("rotation", new CommandOption<Quaternion>());
                description = "Instantiate a physical sphere";

            }

            public override ConsoleOutput Logic()
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var rigidbody = sphere.AddComponent<Rigidbody>();

                RaycastHit hit;
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward); ;
                if (Physics.Raycast(ray,out hit))
                {
                    sphere.transform.position = hit.point;
                }
                else
                {
                    sphere.transform.position = Camera.main.transform.position + Camera.main.transform.forward *5f;

                }

                return new ConsoleOutput("Sphere created at " + sphere.transform.position.ToString(), ConsoleOutput.OutputType.Log);
            }

        }
    }
}