using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
                var fields = command.GetType().GetFields();//Set command options
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (fieldInfo.GetCustomAttribute<CommandParameterAttribute>() != null)
                    {
                        var commandParameterType = typeof(CommandParameter<>);
                        var commandParameterTypeGeneric = commandParameterType.MakeGenericType(fieldInfo.FieldType);
                        var commandParameter = Activator.CreateInstance(commandParameterTypeGeneric, new object[] { command, fieldInfo });
                        var commandParameterAttribute = fieldInfo.GetCustomAttribute<CommandParameterAttribute>();
                        command.commandParameters.Add(commandParameterAttribute.description, (CommandParameter)commandParameter);
                        commandParameterAttribute.commandParameter = (CommandParameter)commandParameter;
                    }
                }
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
                    var keys = command.commandParameters.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        commandList += " [" + keys[i].ToString() + "] ";
                    }

                    commandList += "  --" + command.GetDescription();
                }
                return new ConsoleOutput("Available commands are "+ commandList, ConsoleOutput.OutputType.System);
            }

        }

        [ConsoleCommand("move", "Translate a game object's transform to a world point")]
        class Move : Command
        {
            [CommandParameter("transform")]
            public Transform transform;
            [CommandParameter("position")]
            public Vector3 position;
            public Move()
            {

            }

            public override ConsoleOutput Logic()
            {

                transform.position = position;
                return new ConsoleOutput(((Transform)transform).name + " moved to " + position.ToString() , ConsoleOutput.OutputType.Log);
            }

        }

       
        [ConsoleCommand("rotate", "Rotate a game object")]
        class Rotate : Command
        {
            [CommandParameter("transform")]
            public Transform transform;
            [CommandParameter("rotation")]
            public Quaternion rotation;
            public Rotate()
            {

            }

            public override ConsoleOutput Logic()
            {

                transform.rotation = rotation;
                return new ConsoleOutput(((Transform)transform).name + " rotated to " + rotation.ToString(), ConsoleOutput.OutputType.Log);
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



        [ConsoleCommand("export","Export this session to a text file")]
        class Export : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();

                var outputs = DeveloperConsole.Instance.consoleOutputs;
                var src = DateTime.Now;

                string fileName = "console-"+src.Year + "-" + src.Hour + "-" + src.Minute+".txt";
                string fileContent = "";

                foreach (ConsoleOutput consoleOutput in outputs)
                {
                    fileContent += consoleOutput.output + "\n";
                }

                string filePath = Directory.GetParent(Application.dataPath)+"/Logs/" + fileName;
             
                StreamWriter streamWriter = new StreamWriter(filePath,true);

                
                streamWriter.Write(fileContent);

                streamWriter.Close();


                return new ConsoleOutput("Log file created at '" + filePath + "'.", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("beep", "Play the sound associated with the Beep system event")]
        class Beep : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                System.Media.SystemSounds.Beep.Play();
                return new ConsoleOutput("Beeping for 6 seconds.", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("quit", "Exit the application")]
        class Quit : Command
        {

            public override ConsoleOutput Logic()
            {
                base.Logic();
                Application.Quit();
                return new ConsoleOutput("Have a very safe and productive day.", ConsoleOutput.OutputType.Log);
            }
        }


        [ConsoleCommand("echo", "Echo text to console.")]
        class Echo : Command
        {
            [CommandParameter("string")]
            public string echoText;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                
                return new ConsoleOutput(echoText, ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("fps_max", "Limit the frame rate. Set 0 for unlimited")]
        class Fps_max : Command
        {
            [CommandParameter("maxFPS")]
            public int maxFPS;
            public override ConsoleOutput Logic()
            {
                base.Logic();
                Application.targetFrameRate = maxFPS;
                return new ConsoleOutput("Frame rate limited to " + maxFPS+" frames per second.", ConsoleOutput.OutputType.Log);
            }
        }
        [ConsoleCommand("screenshot", "Save a screenshot")]
        class screenshot : Command
        {
            
            public override ConsoleOutput Logic()
            {
                base.Logic();
                var src = DateTime.Now;
                string fileName = "screenshot-" + src.Year + "-" + src.Hour + "-" + src.Minute + ".png";
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) +"/"+fileName;
                ScreenCapture.CaptureScreenshot(filePath);
                return new ConsoleOutput("Screenshot saved to " + filePath + ".", ConsoleOutput.OutputType.Log);
            }
        }

        [ConsoleCommand("loadlevel", "Load the level by given name or id")]
        class LoadLevel : Command
        {
            [CommandParameter("level")]
            public string targetLevel;

            public override ConsoleOutput Logic()
            {
                base.Logic();
                var levelId = 0;
                if (int.TryParse(targetLevel, out levelId))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(levelId);
                    return new ConsoleOutput("Loading level with id " + levelId + ".", ConsoleOutput.OutputType.Log);

                }
                
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetLevel);
                    return new ConsoleOutput("Loading level " + targetLevel + ".", ConsoleOutput.OutputType.Log);

            }
        }
        #endregion
    }

}