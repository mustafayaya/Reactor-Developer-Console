using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Console{
    public class DeveloperConsole : MonoBehaviour
    {

        public static DeveloperConsole _instance;

        [Header("Console Settings")]
        public bool active = true;
        public GUISkin skin;
        public int lineSpacing = 20;
        public int inputLimit = 64;
        public Color logOutputColor = Color.white;
        public Color warningOutputColor = Color.yellow;
        public Color errorOutputColor = Color.red;
        public Color networkOutputColor = Color.cyan;
        public string submitInputAxis = "Submit";

        Commands commands;

        List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>();
        private Vector2 scrollPosition = Vector2.zero;
        private Rect windowRect = new Rect(200, 200, Screen.width * 50 / 100, Screen.height * 60 / 100);
        string input = "Command here";

        public static DeveloperConsole Instance
        {
            get
            {
                return FindObjectOfType<DeveloperConsole>();
            }
        }
       
        public void Start()
        {
            commands = new Commands();
            _instance = this;
        }

        public void InputSubmit(string _input)
        {
            InputQuery(_input);
            input = "";

        }

        public void InputQuery(string _input)
        {
            foreach (Command command in commands.GetCommands())
            {
                if (command.queryIdentity == _input)
                {
                    Execute(command);
                    return;
                }
            }
            WriteLog("There is no command such as '" + _input+"'");
        }

        public void Execute(Command command)
        {
            var output = command.Logic(); //Execute the command and get output
            Write(output);
        }

        public static bool WriteLog(string input)
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Log);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteWarning(string input)
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Warning);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteError(string input)
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Error);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public bool WriteNetwork(string input)
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Network);
            consoleOutputs.Add(output);
            return true;
        }
        public bool Write(ConsoleOutput consoleOutput)
        {
            consoleOutputs.Add(consoleOutput);
            return true;
        }

        void OnGUI()
        {
            if (active)
            {
                windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console", skin.window);
            }

        }
        KeyCode _keyCode;
        void ConsoleWindow(int windowID)
        {

            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
            int scrollHeight = 0;
            GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), "", skin.box);


            foreach (ConsoleOutput c in consoleOutputs)
            {
                scrollHeight += c.lines * lineSpacing;
            }

            scrollPosition = GUI.BeginScrollView(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), scrollPosition, new Rect(20, 20, windowRect.width - 60, scrollHeight));

            for (int i = 0; i < consoleOutputs.Count; i++)
            {
                int space = 0;
                foreach (ConsoleOutput c in consoleOutputs)
                {
                    if (consoleOutputs.IndexOf(c) < i)
                    {
                        space += c.lines * lineSpacing;
                    }
                }
                //Debug.Log("Space :" + space + "Lines : " + consoleOutputs.Count);
                ConsoleOutputText(new Rect(20, 20 + space, windowRect.width - 20, lineSpacing), consoleOutputs[i], skin.label);
            }
            GUI.EndScrollView();
            GUI.SetNextControlName("consoleInputField");
            input = GUI.TextField(new Rect(20, windowRect.height - 45, windowRect.width - 160, 25), input, inputLimit, skin.textField);


            if (GUI.Button(new Rect(windowRect.width - 130, windowRect.height - 45, 80, 25), "Submit", skin.button))
            {
                InputSubmit(input);
                scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
            }
            if (GUI.Button(new Rect(windowRect.width - 40, windowRect.height - 45, 20, 25), "X", skin.button))
            {
                consoleOutputs.Clear();
                scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
            }
            if (!String.IsNullOrEmpty(input) && Event.current.keyCode == KeyCode.Return && Event.current.keyCode != _keyCode)
            {

                InputSubmit(input);
            } else if (String.IsNullOrEmpty(input) && Event.current.keyCode == KeyCode.Return)
            {
                GUI.FocusControl("consoleInputField");

            }
            _keyCode = Event.current.keyCode;

        }

        string ConsoleOutputText(Rect position, ConsoleOutput consoleOutput, GUIStyle style)
        {
            style.font.RequestCharactersInTexture(consoleOutput.output, style.fontSize, style.fontStyle);
            int _labelWidth = 0;

            foreach (char c in consoleOutput.output.ToCharArray())
            {
                CharacterInfo characterInfo;
                style.font.GetCharacterInfo(c, out characterInfo, style.fontSize);
                _labelWidth += (int)characterInfo.width;
            }

            int lines = (int)Mathf.Clamp(Mathf.Floor(_labelWidth / position.width), 1, 128);
            if (lines != 1)//If there is more than one line, fix the spacing bug with adding more lines.
            {
                if (lines == 2)
                {
                    lines += 1;
                }
                else
                {
                    lines += 2;
                }
            }
            var stringLines = consoleOutput.output.Split('\n').Length; //Count the lines
            lines += stringLines;
            consoleOutput.lines = lines;
            switch (consoleOutput.outputType) {
                case ConsoleOutput.OutputType.Log:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = logOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Warning:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = warningOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Error:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = errorOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Network:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = networkOutputColor } });
                    break;
            }
            return null;
        }


    }
}