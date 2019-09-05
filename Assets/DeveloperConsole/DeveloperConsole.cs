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
        private static DeveloperConsole _instance;
       [Header("Console Settings")]
        public bool active = true; //If active, draw console on UI
        public bool printLogs = true;//If active, print logs
        public bool printWarnings = true;//If active, print warnings
        public bool printErrors = true;//If active, print errors
        public bool printNetwork = true;//If active, print network
        private bool _predictions;

        public bool predictions//If active, enable prediction widget
        {
            set { _predictions = value; Widgets.Instance.enabled = value; }
            get { return _predictions; }
        }
        public GUISkin skin;//If active, print network
        public int lineSpacing = 20;//Set spacing between output lines
        public int inputLimit = 64;//Set maximum console input limit
        public Color userOutputColor = Color.HSVToRGB(0,0,0.75f);//Set color of user outputs
        public Color systemOutputColor = Color.HSVToRGB(0, 0, 0.80f);//Set color of system outputs
        public Color logOutputColor = Color.HSVToRGB(0, 0, 0.90f);//Set color of log outputs
        public Color warningOutputColor = Color.yellow;//Set color of warning outputs
        public Color errorOutputColor = Color.red;//Set color of error outputs
        public Color networkOutputColor = Color.cyan;//Set color of network outputs
        Commands commands;//Private field for commands script
        List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>();//List outputs here
        private Vector2 scrollPosition = Vector2.zero;//Determine output window's scroll position
        private Rect windowRect = new Rect(200, 200, Screen.width * 50 / 100, Screen.height * 60 / 100);//TODO: Make window rect dynamic
        public string input = "help";//Describe input string for console input field. Set default text here.

        public static DeveloperConsole Instance//Singleton
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                return FindObjectOfType<DeveloperConsole>();
            }
        }

        public void Start()
        {
            commands = Commands.Instance;//Get commands script
            WriteSystem("Welcome");
       
        }

        public void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))//inputFocusTrigger gets true when user presses enter on a prediction button. But if user clicks, make this trigger false
            {
                inputFocusTrigger = false;
            }
            OutputFilterHandler();
            predictions = true;
        }

        private void OutputFilterHandler()
        {
            if (!printErrors)
            {
                if (consoleOutputs.Find(x => x.outputType == ConsoleOutput.OutputType.Error) != null)
                {
                    consoleOutputs.RemoveAll(x => x.outputType == ConsoleOutput.OutputType.Error);
                }
            }
            if (!printLogs)
            {
                if (consoleOutputs.Find(x => x.outputType == ConsoleOutput.OutputType.Log) != null)
                {
                    consoleOutputs.RemoveAll(x => x.outputType == ConsoleOutput.OutputType.Log);
                }
            }
            if (!printNetwork)
            {
                if (consoleOutputs.Find(x => x.outputType == ConsoleOutput.OutputType.Network) != null)
                {
                    consoleOutputs.RemoveAll(x => x.outputType == ConsoleOutput.OutputType.Network);
                }
            }
            if (!printWarnings)
            {
                if (consoleOutputs.Find(x => x.outputType == ConsoleOutput.OutputType.Warning) != null)
                {
                    consoleOutputs.RemoveAll(x => x.outputType == ConsoleOutput.OutputType.Warning);
                }
            }
        }

        public void SubmitInput(string _input)//Submit input string to console query
        {
            _input.Trim();
            WriteUser("} " + _input );
            QueryInput(_input);
            inputHistory.Add(_input);
            input = "";
            _historyState = -1;
            submitFocusTrigger = true;
        }


        public void QueryInput(string _input)//Make query with given input
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
                                    WriteSystem("Parameter [" + keys[i] + "] is given wrong.");
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
                                    WriteSystem("Parameter [" + keys[i] + "] is given wrong.");
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
                                    WriteSystem("Parameter [" + keys[i] + "] is given wrong.");
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
            WriteSystem("There is no command such as '" + _input + "'");
        }

        public void Execute(Command command)
        {
            var output = command.Logic(); //Execute the command and get output
            Write(output);
        }

        public static bool WriteUser(string input)//Write simple line
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.User, false);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteSystem(string input)//Write simple line
        {
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.System);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteLine(string input)//Write simple line
        {
            if (!Instance.printLogs)
            {
                return false;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if(Instance.consoleOutputs.Last().output == input)
                {
                    return false;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Log, false);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteLog(string input)
        {
            if (!Instance.printLogs)
            {
                return false;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input)
                {
                    return false;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Log);
            Instance.consoleOutputs.Add(output);
            return true;
        }

        public static bool WriteWarning(string input)
        {
            if (!Instance.printWarnings)
            {
                return false;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input)
                {
                    return false;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Warning);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteError(string input)
        {
            if (!Instance.printErrors)
            {
                return false;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input)
                {
                    return false;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Error);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public static bool WriteNetwork(string input)
        {
            if (!Instance.printNetwork)
            {
                return false;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input)
                {
                    return false;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input, ConsoleOutput.OutputType.Network);
            Instance.consoleOutputs.Add(output);
            return true;
        }
        public bool Write(ConsoleOutput consoleOutput)
        {
            consoleOutputs.Add(consoleOutput);
            return true;
        }




        public object ParamQuery(Type t, string parameter)//Make query with given parameter and type
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
        public void InputHistoryHandler()//Restore saved inputs 
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
            if (active)//If active, draw console window
            {
                GUI.depth = 1;

                windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console", skin.window);
            }
     
        }

        void ConsoleWindow(int windowID)
        {


            printLogs = GUI.Toggle(new Rect(10, 4, 10, 10), printLogs, "", skin.GetStyle("logButton"));
            printWarnings = GUI.Toggle(new Rect(25, 4, 10, 10), printWarnings, "", skin.GetStyle("warningButton"));
            printErrors = GUI.Toggle(new Rect(40, 4, 10, 10), printErrors, "", skin.GetStyle("errorButton"));
            printNetwork = GUI.Toggle(new Rect(55, 4, 10, 10), printNetwork, "", skin.GetStyle("networkButton"));

            if (GUI.Button(new Rect(windowRect.width - 20, 4, 10, 10),"X", skin.GetStyle("exitButton")))
            {
                active = !active;
            }

            if (Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.KeyDown)
            {
                if (GUI.GetNameOfFocusedControl() == "consoleInputField")
                {
                    InputHistoryHandler();
                    FocusOnInputField(false);
                }
            }

            GUI.SetNextControlName("dragHandle");

            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));//Draw drag handle of window
            int scrollHeight = 0;
            GUI.SetNextControlName("outputBox");

            GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), "", skin.box);//Draw console window box

            GUI.SetNextControlName("consoleInputField");
            Rect inputFieldRect = new Rect(20, windowRect.height - 45, windowRect.width - 160, 25);
            Widgets.Instance.DrawCommandHints(inputFieldRect, skin.GetStyle("hint"));
            input = GUI.TextField(inputFieldRect, input, inputLimit, skin.textField);


            foreach (ConsoleOutput c in consoleOutputs)
            {
                scrollHeight += c.lines * lineSpacing;
            }

            scrollPosition = GUI.BeginScrollView(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), scrollPosition, new Rect(20, 20, windowRect.width - 60, scrollHeight));
            GUI.SetNextControlName("textArea");
            DrawOutput();
            
            GUI.EndScrollView();

            GUI.SetNextControlName("submitButton");

            if ( GUI.Button(new Rect(windowRect.width - 130, windowRect.height - 45, 80, 25), "Submit", skin.button))
            {
                if (!String.IsNullOrEmpty(input))
                {
                    SubmitInput(input);
                    scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 40);
                }
               
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
                        SubmitInput(input);

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

            GUI.Box(new Rect(windowRect.width - 15, windowRect.height - 15, 10, 10), "", skin.GetStyle("corner"));
            
            WindowResizeHandler();
        }

        private void DrawOutput()
        {
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
        }

        bool handleClicked = false;
        Vector3 clickedPosition;
        Rect _window;
        private void WindowResizeHandler()
        {
            var mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;    // Convert to GUI coords
            var windowHandle = new Rect(windowRect.x + windowRect.width - 20, windowRect.y + windowRect.height - 20, 20, 20);
                // If clicked on window resize widget
            if (Input.GetMouseButtonDown(0) && windowHandle.Contains(mousePos)) {
                handleClicked = true;
                clickedPosition = mousePos;
                _window = windowRect;
            }

            if (handleClicked)
            {
                // Resize window by dragging
                if (Input.GetMouseButton(0))
                {
                    windowRect.width = Mathf.Clamp(_window.width + (mousePos.x - clickedPosition.x), 300, Screen.width);
                    windowRect.height = Mathf.Clamp(_window.height + (mousePos.y - clickedPosition.y), 200, Screen.height);
                }
                // Finish resizing window
                if (Input.GetMouseButtonUp(0))
                {
                    handleClicked = false;
                }
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
            var output = consoleOutput.output + "\n";
            var outputLines = output.Split('\n');

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
                case ConsoleOutput.OutputType.User:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = userOutputColor } });
                    break;
                case ConsoleOutput.OutputType.System:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = systemOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Log:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = logOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Warning:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = warningOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Error:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = errorOutputColor } });
                    break;
                case ConsoleOutput.OutputType.Network:
                    GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.dateTime + consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = networkOutputColor } });
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

        public Rect GetWindowRect()
        {
            return windowRect;
        }
    }
}