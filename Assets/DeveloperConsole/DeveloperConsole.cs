using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Console
{
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
        string input = "help";

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

        public void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))//inputFocusTrigger gets true when user presses enter on a prediction button. But if user clicks, make this trigger false
            {
                inputFocusTrigger = false;
            }
        }

        public void InputSubmit(string _input)
        {
            _input.Trim();
            WriteLine("} " + _input + "\n");
            InputQuery(_input);
            inputHistory.Add(_input);
            input = "";
            _historyState = -1;
            submitFocusTrigger = true;
        }


        public void InputQuery(string _input)
        {

            foreach (Command command in commands.GetCommands())
            {
                var _inputParams = GetInputParameters(_input);

                if (((ConsoleCommandAttribute)Attribute.GetCustomAttribute(command.GetType(), typeof(ConsoleCommandAttribute))).queryIdentity == _inputParams[0])
                {
                    if (_inputParams.Length != 1)
                    {
                        var keys = command.commandOptions.Keys.ToArray();

                        for (int i = 0; i < keys.Length; i++)
                        {
                            Type genericTypeArgument = command.commandOptions[keys[i]].GetType().GenericTypeArguments[0];

                            if (genericTypeArgument.IsSubclassOf(typeof(Component)))
                            {
                                Type t = command.commandOptions[keys[i]].genericType.GetType();
                                var query = (ParamQuery(command.commandOptions[keys[i]].genericType, _inputParams[i + 1]));
                                if (query != null)
                                {
                                    command.commandOptions[keys[i]].optionParameter = query;
                                }
                                else
                                {
                                    WriteLog("Parameter [" + keys[i] + "] is given wrong.");
                                    return;
                                }

                            }
                            if (command.commandOptions[keys[i]] is CommandOption<Vector3>)
                            {
                                Vector3 query = Vector3.zero;
                                if (Utility.GetVector3FromString(_inputParams[i + 1],out query)){
                                    ((command.commandOptions[keys[i]]) as CommandOption<Vector3>).optionParameter = query;
                                }
                                else
                                {
                                    WriteLog("Parameter [" + keys[i] + "] is given wrong.");
                                    return;
                                }

                            }
                            if (command.commandOptions[keys[i]] is CommandOption<Quaternion>)
                            {
                                var query = Quaternion.identity;
                                if (Utility.GetQuaternionFromString(_inputParams[i + 1], out query))
                                {
                                    ((command.commandOptions[keys[i]]) as CommandOption<Quaternion>).optionParameter = query;
                                }
                                else
                                {
                                    WriteLog("Parameter [" + keys[i] + "] is given wrong.");
                                    return;
                                }

                            }
                        }
                        Execute(command);
                        return;
                    }
                    Execute(command);
                    return;
                }

            }
            WriteLog("There is no command such as '" + _input + "'");
        }

        public void Execute(Command command)
        {
            var output = command.Logic(); //Execute the command and get output
            Write(output);
        }

        public static bool WriteLine(string input)
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Log, false);
            Instance.consoleOutputs.Add(output);
            return true;
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

        public object ParamQuery(Type t, string parameter)
        {

            if (t.IsSubclassOf(typeof(Component)))
            {
                Component query = null;

                var go = GameObject.Find(parameter);
                if (go != null)
                { query = go.GetComponent(t); }

                return query;
            }
            return null;
        }


        int _historyState = -1;
        public void RestoreInput()
        {
            if (_historyState >= inputHistory.Count)
            {
                _historyState = inputHistory.Count - 1;
            }
                if (inputHistory.Count != 0)
                {
                    if (_historyState == -1)
                    {
                        _historyState = inputHistory.Count - 1;
                    }
                    else
                    {
                    if (_historyState == 0)
                    {
                        _historyState =inputHistory.Count - 1;
                    }
                    else
                    {
                        _historyState--;

                    }
                }
                }

                if (_historyState != -1)
                {
                    input = inputHistory[Mathf.Clamp(_historyState, -1, inputHistory.Count)];
                }
        }

        public List<string> inputHistory = new List<string>();
        bool submitFocusTrigger;

        void OnGUI()
        {
            if (active)
            {
                windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console", skin.window);
            }

    

        }

        bool moveInputEnd;
        void ConsoleWindow(int windowID)
        {
         

            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
            int scrollHeight = 0;
            GUI.SetNextControlName("outputBox");

            GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), "", skin.box);


            foreach (ConsoleOutput c in consoleOutputs)
            {
                scrollHeight += c.lines * lineSpacing;
            }

            scrollPosition = GUI.BeginScrollView(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), scrollPosition, new Rect(20, 20, windowRect.width - 60, scrollHeight));
            GUI.SetNextControlName("textArea");

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



            if (Event.current.keyCode == KeyCode.DownArrow && Event.current.type == EventType.KeyUp)
            {
                if (GUI.GetNameOfFocusedControl() == "consoleInputField")
                {
                    RestoreInput();
                    FocusOnInputField(false);
                }
            }


            GUI.SetNextControlName("consoleInputField");
            input = GUI.TextField(new Rect(20, windowRect.height - 45, windowRect.width - 160, 25), input, inputLimit, skin.textField);

            GUI.SetNextControlName("submitButton");

            if ( GUI.Button(new Rect(windowRect.width - 130, windowRect.height - 45, 80, 25), "Submit", skin.button))
            {
                if (!String.IsNullOrEmpty(input))
                {
                    InputSubmit(input);
                    scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 40);
                }
               
            }


            if (!String.IsNullOrEmpty(input) && GUI.GetNameOfFocusedControl() != "")
            {
                CommandPredictionQuery();
            }
            if (inputFocusTrigger)
            {
                FocusOnInputField(false);
                
            }
            if (!String.IsNullOrEmpty(input) && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp )
            {
                
                if (GUI.GetNameOfFocusedControl() == "consoleInputField" || GUI.GetNameOfFocusedControl() == "")
                {
                    if (inputFocusTrigger)
                    {
                        inputFocusTrigger = false;
                    }
                    else
                    {
                        InputSubmit(input);

                    }

                }

            }
            GUI.SetNextControlName("clearButton");

            if (GUI.Button(new Rect(windowRect.width - 40, windowRect.height - 45, 20, 25), "X", skin.button))
            {
                consoleOutputs.Clear();
                inputHistory.Clear();
                scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
            }

            if (String.IsNullOrEmpty(input) && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
            {

                GUI.FocusControl("consoleInputField");

            }
            if (GUI.GetNameOfFocusedControl() == "" && submitFocusTrigger)
            {
                GUI.FocusControl("consoleInputField");
                submitFocusTrigger = false;
            }
            if (GUI.GetNameOfFocusedControl() == "clearButton")
            {
                GUI.FocusControl("consoleInputField");

            }
  
        }


        int predictionSelectionState = 0;
        string _lastPredictionQueryInput;
        List<string> predictedCommandIdentities = new List<string>();

        public void CommandPredictionQuery() //Predict commands and print them
        {
            if (_lastPredictionQueryInput != input) {
                predictedCommandIdentities.Clear();
                foreach (Command command in commands.GetCommands())//Check every command and compare them to input
                {
                    if (input.Length < command.GetQueryIdentity().Length)
                    {
                        if (command.GetQueryIdentity().Substring(0, input.Length) == input)
                        {
                            Debug.Log(command.GetQueryIdentity());

                            predictedCommandIdentities.Add(command.GetQueryIdentity());
                        }
                    }

                }
                _lastPredictionQueryInput = input;
            }
            predictedCommandIdentities.Sort();
            int drawnFields = 0;

            if (Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.KeyUp)
            {
                predictionSelectionState++;

            }
            if (Event.current.keyCode == KeyCode.DownArrow && Event.current.type == EventType.KeyUp)
            {
                predictionSelectionState--;
                if (predictionSelectionState == 0)
                {
                    FocusOnInputField(false);

                }
            }

            predictionSelectionState = Mathf.Clamp(predictionSelectionState, 0, predictedCommandIdentities.Count);


            for (int i = Mathf.Clamp(predictionSelectionState - 5,0,128); i < Mathf.Clamp(predictionSelectionState - 5, 0, 128) + 5 && i < predictedCommandIdentities.Count; i++) { 
                GUI.SetNextControlName("predictedCommand"+i);
                if(GUI.Button(new Rect(20, windowRect.height -95 -((0 - i + Mathf.Clamp(predictionSelectionState - 5, 0, 128) + 1) * -25 ), windowRect.width /4, 25),predictedCommandIdentities[i], skin.GetStyle("prediction")))
                {
                    FocusOnInputField(true);
                    input = (predictedCommandIdentities[i]);

                }
                drawnFields++;
            }

            

            if (predictionSelectionState!= 0)
            {
                GUI.FocusControl("predictedCommand"+(predictionSelectionState - 1).ToString());
               
            }

        }

        bool inputFocusTrigger;

        public void FocusOnInputField(bool blockInput)
        {
            if (blockInput)//If enter gets pressed, it can trigger submit. This trigger blocks unnecessary submissions
            {
                inputFocusTrigger = true;

            }
            GUI.FocusControl("consoleInputField");
            StartCoroutine(FocusOnInputFieldAfterFrame());
        }
        IEnumerator FocusOnInputFieldAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectNone();
            ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).MoveTextEnd();
        }



        string ConsoleOutputText(Rect position, ConsoleOutput consoleOutput, GUIStyle style)
        {
            style.font.RequestCharactersInTexture(consoleOutput.output, style.fontSize, style.fontStyle);

            var outputLines = consoleOutput.output.Split('\n');

            int lines = 0;
            foreach (string line in outputLines)
            {
                int _labelWidth = 0;

                var charArray = line.ToCharArray();
                foreach (char c in charArray)
                {
                    CharacterInfo characterInfo;
                    style.font.GetCharacterInfo(c, out characterInfo, style.fontSize);
                    _labelWidth += (int)characterInfo.width;
                }
                lines += (int)Mathf.Clamp(Mathf.Floor(_labelWidth / position.width), 0, 128);
            }

            lines += outputLines.Count();
            consoleOutput.lines = lines;

            switch (consoleOutput.outputType)
            {
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

        private string[] GetInputParameters(string _input)
        {
            List<string> _inputParams = new List<string>();
            int paramCount = 0;
            var charList = _input.ToCharArray();
            bool readingParam = false;
            string _param = "";
            for (int i = 0; i< _input.Length; i++)
            {
                if (charList[i] == '(' && !readingParam)
                {
                    readingParam = true;
                }
                if (charList[i] != ')' && readingParam && charList[i] != '(')
                {
                    _param += charList[i].ToString();

                }

                if (charList[i] != ' ' && !readingParam)
                {
                    _param += charList[i].ToString();

                }
                if (charList[i] == ' ' && !readingParam)
                {
                    _inputParams.Add(_param);
                    _param = "";
                    paramCount += 1;
                }
                if (charList[i] == ')' && readingParam)
                {
                    readingParam = false;

                }
                if (i == _input.Length -1 && !String.IsNullOrEmpty(_param))
                {
                    _inputParams.Add(_param);
                    _param = "";
                    paramCount += 1;
                }
            }

            foreach (string s in _inputParams)
            {
                s.Trim();
            }
            return _inputParams.ToArray();
        }


    }
}