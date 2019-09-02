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

        #region variables
        private static Commands instance = null;


        private List<Command> _commands = new List<Command>();

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

        #endregion

        #region Initialization

        public Commands()
        {
            RegisterCommands();
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
                if (String.IsNullOrEmpty(((ConsoleCommandAttribute)Attribute.GetCustomAttribute(c.GetType(), typeof(ConsoleCommandAttribute))).queryIdentity))
                {
                    var message = "Command " + c + "("+c.GetHashCode()+") doesn't has a query identity. It will be ignored." ;
                    Console.DeveloperConsole.WriteWarning(message);
                    _commands.Remove(c);
                }
            }
            
        }

        #endregion

        #region commands
        [ConsoleCommand("help", "List all available commands")]
        class Help : Command
        {
            public Help()
            {

            }

            public override ConsoleOutput Logic()
            {
                string commandList = "\n";

                foreach (Command command in Commands.Instance.GetCommands())
                {
                    commandList = commandList + "\n -" + command.GetQueryIdentity();
                    var keys = command.commandOptions.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        commandList += " [" + keys[i].ToString() + "] ";
                    }

                    commandList += "  --" + command.GetDescription();

                }
                return new ConsoleOutput("Available commands are "+ commandList, ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand("move", "Translate a game object's transform to a world point")]
        class Move : Command
        {
             public Move()
            {
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
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Transform>(), ConsoleOutput.OutputType.Log);

                }
                if (vec == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Quaternion>(), ConsoleOutput.OutputType.Log);

                }
                trans.position = vec;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + vec.ToString() , ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand("moove", "Translate a game object's transform to a world point")]
        class Moove : Command
        {
            public Moove()
            {
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("position", new CommandOption<Vector3>());
            }

            public override ConsoleOutput Logic()
            {
                var trans = (Transform)((CommandOption)(commandOptions["transform"] as CommandOption<Transform>)).optionParameter;
                var vec = (commandOptions["position"] as CommandOption<Vector3>).optionParameter;

                //Debug.Log("transported");
                if (trans == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Transform>(), ConsoleOutput.OutputType.Log);

                }
                if (vec == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Quaternion>(), ConsoleOutput.OutputType.Log);

                }
                trans.position = vec;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + vec.ToString(), ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand("moorove", "Translate a game object's transform to a world point")]
        class Moorove : Command
        {
            public Moorove()
            {
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("position", new CommandOption<Vector3>());
            }

            public override ConsoleOutput Logic()
            {
                var trans = (Transform)((CommandOption)(commandOptions["transform"] as CommandOption<Transform>)).optionParameter;
                var vec = (commandOptions["position"] as CommandOption<Vector3>).optionParameter;

                //Debug.Log("transported");
                if (trans == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Transform>(), ConsoleOutput.OutputType.Log);

                }
                if (vec == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Quaternion>(), ConsoleOutput.OutputType.Log);

                }
                trans.position = vec;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + vec.ToString(), ConsoleOutput.OutputType.Log);
            }

        }


        [ConsoleCommand("rotate", "Rotate a game object")]
        class Rotate : Command
        {
            public Rotate()
            {
                commandOptions.Add("transform", new CommandOption<Transform>());
                commandOptions.Add("rotation", new CommandOption<Quaternion>());

            }

            public override ConsoleOutput Logic()
            {
                var trans = (Transform)((CommandOption)(commandOptions["transform"] as CommandOption<Transform>)).optionParameter;
                var quaternion = (commandOptions["rotation"] as CommandOption<Quaternion>).optionParameter;

                if (trans == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Transform>(), ConsoleOutput.OutputType.Log);

                }
                if (quaternion == null)
                {
                    return new ConsoleOutput(Console.Utility.ParamsGivenWrong<Quaternion>(), ConsoleOutput.OutputType.Log);

                }
                trans.rotation = quaternion;
                return new ConsoleOutput(((Transform)trans).name + " moved to " + quaternion.ToString(), ConsoleOutput.OutputType.Log);
            }

        }

        [ConsoleCommand("sphere", "Instantiate a physical sphere")]
        class Sphere : Command
        {
            public Sphere()
            {

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



        #endregion
    }

}