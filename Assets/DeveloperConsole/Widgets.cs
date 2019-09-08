using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Console
{
    class Widgets : MonoBehaviour
    {
        private static Widgets _instance;//Singleton
        public static Widgets Instance//Singleton
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                _instance = DeveloperConsole.Instance.gameObject.AddComponent<Widgets>();
                return _instance;
            }
        }

        public void OnGUI()
        {

            if (!String.IsNullOrEmpty(DeveloperConsole.Instance.input) && GUI.GetNameOfFocusedControl() != "")
            {
                CommandPredictionQuery();
            }
        }



        string _lastPredictionQueryInput;
        int predictionSelectionState = 0;
        List<string> predictedCommandIdentities = new List<string>();
        List<Command> commands;
 


        private void CommandPredictionQuery() //Predict commands and print them
        {
            if (commands == null)
            {
                commands = Commands.Instance.GetCommandsSingle();
            }
            GUI.depth = -1;
            DeveloperConsole developerConsole = DeveloperConsole.Instance;
            var input = developerConsole.input;

            var windowRect = developerConsole.GetWindowRect();
            Rect inputFieldRect = new Rect(windowRect.x + 20, windowRect.y + windowRect.height - 45, windowRect.width - 160, 25);

            if (_lastPredictionQueryInput != input)
            {
                predictedCommandIdentities.Clear();
                foreach (Command command in commands)//Check every command and compare them to input
                {
                    if (input.Length < command.GetQueryIdentity().Length)
                    {
                        if (command.GetQueryIdentity().Substring(0, input.Length).ToLower() == input.ToLower())
                        {

                            predictedCommandIdentities.Add(command.GetQueryIdentity());
                        }
                    }

                }
                _lastPredictionQueryInput = input;
            }
            predictedCommandIdentities.Sort();
            int drawnFields = 0;

            if (Event.current.keyCode == KeyCode.DownArrow && Event.current.type == EventType.KeyUp)
            {
                predictionSelectionState++;

            }
            if (Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.KeyUp)
            {
                predictionSelectionState--;
                if (predictionSelectionState == 0)
                {
                    developerConsole.FocusOnInputField(false);

                }
            }

            predictionSelectionState = Mathf.Clamp(predictionSelectionState, 0, predictedCommandIdentities.Count);


            for (int i = Mathf.Clamp(predictionSelectionState - 5, 0, 128); i < Mathf.Clamp(predictionSelectionState - 5, 0, 128) + 5 && i < predictedCommandIdentities.Count; i++)
            {
                GUI.SetNextControlName("predictedCommand" + i);
                developerConsole.skin.GetStyle("prediction").font.RequestCharactersInTexture(predictedCommandIdentities[i], developerConsole.skin.GetStyle("prediction").fontSize, developerConsole.skin.GetStyle("prediction").fontStyle);

                if (GUI.Button(new Rect(inputFieldRect.x, inputFieldRect.y + 2*inputFieldRect.height + ((0 - i + Mathf.Clamp(predictionSelectionState - 5, 0, 128) + 1) * -25), windowRect.width / 4, 25), predictedCommandIdentities[i], developerConsole.skin.GetStyle("prediction")))
                {
                    developerConsole.FocusOnInputField(true);
                    developerConsole.input = (predictedCommandIdentities[i]);

                }
                drawnFields++;
            }



            if (predictionSelectionState != 0)
            {
                GUI.FocusControl("predictedCommand" + (predictionSelectionState - 1).ToString());

            }
        }

        string _lastInput;
        string _hintText;

        public void DrawCommandHints(Rect inputFieldRect,GUIStyle hintStyle)//Draw hint label on input field
        {

           DeveloperConsole developerConsole = DeveloperConsole.Instance;

            var input = developerConsole.input;
            if (_lastInput != input)
            {
                if (!String.IsNullOrEmpty(input))
                {
                    foreach (Command command in Commands.Instance.GetCommands())//Check every command and compare them to input
                    {

                        if (input == command.GetQueryIdentity())
                        {
                            string hintText = "";
                            for (int i = 0; i < command.GetQueryIdentity().Length; i++)
                            {
                                hintText += " ";
                            }
                            var parameterFields = command.GetType().GetFields();

                            foreach (FieldInfo fieldInfo in parameterFields)
                            {
                                if (fieldInfo.GetCustomAttribute<CommandParameterAttribute>() != null)
                                {
                                    hintText += " [" + fieldInfo.GetCustomAttribute<CommandParameterAttribute>().description + "]";

                                }

                            }
                            _hintText = hintText;
                            _lastInput = input;

                            return;
                        }

                    }
                    _hintText = "";

                }
                _lastInput = input;
            }
            if (!String.IsNullOrEmpty(_hintText)&& !String.IsNullOrEmpty(input))
            {
                GUI.Label(inputFieldRect,_hintText, hintStyle);

            }

        }
    }
}
