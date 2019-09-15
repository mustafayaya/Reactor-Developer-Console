/*
 * Reactor Developer Console
 * Developed by Mustafa Yaya @2019 
 * 
 * Licence: MIT
 * 
 * GitHub
 * https://mustafayaya.github.io/Reactor-Developer-Console/
 * 
 * Unity Forum
 * https://forum.unity.com/threads/reactor-open-source-developer-console.741614/
 */

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

        [SerializeField]
        private bool _active = true;
        public static bool active
        {
            get { return DeveloperConsole.Instance._active; }
            set { DeveloperConsole.Instance._active = value; }
        }

        public bool printLogs = true;
        public bool printWarnings = true;
        public bool printErrors = true;
        public bool printNetwork = true;
        private bool _predictions;
        public bool printUnityConsole;
        public bool predictions//If active, enable prediction widget
        {
            set { _predictions = value; Widgets.Instance.enabled = value; }
            get { return _predictions; }
        }
        public GUISkin skin;
        public int lineSpacing = 20;//Set spacing between output lines
        public int inputLimit = 64;//Set maximum console input limit
        public Color userOutputColor = Color.HSVToRGB(0, 0, 0.75f);
        public Color systemOutputColor = Color.HSVToRGB(0, 0, 0.90f);
        public Color logOutputColor = Color.HSVToRGB(0, 0, 0.90f);
        public Color warningOutputColor = Color.yellow;
        public Color errorOutputColor = Color.red;
        public Color networkOutputColor = Color.cyan;
        Commands commands;//Private field for commands script
        public List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>();//List outputs here
        private Vector2 scrollPosition = Vector2.zero;//Determine output window's scroll position
        private bool scrollDownTrigger;
        private Rect windowRect;
        public string input = "help";//Describe input string for console input field. Set default text here.
        public bool drawCloseButtonOnMobile = true;
        private static bool created = false;

        #region statics
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


        public void Write(ConsoleOutput consoleOutput)//Write simple line
        {
            consoleOutputs.Add(consoleOutput);
            Instance.scrollDownTrigger = true;
        }


        public static void WriteUser(object input)//Write user line without time tag
        {
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.User, false);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }
        public static void WriteSystem(object input)
        {
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.System);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }
        public static void WriteLine(object input)//Write line without time tag
        {
            if (!Instance.printLogs)
            {
                return;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input.ToString())
                {
                    return;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.Log, false);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }
        public static void WriteLog(object input)
        {
            if (!Instance.printLogs)
            {
                return;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input.ToString())
                {
                    return;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.Log);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }

        public static void WriteWarning(object input)
        {
            if (!Instance.printWarnings)
            {
                return;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input.ToString())
                {
                    return;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.Warning);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }
        public static void WriteError(object input)
        {
            if (!Instance.printErrors)
            {
                return;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input.ToString())
                {
                    return;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.Error);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }
        public static void WriteNetwork(object input)
        {
            if (!Instance.printNetwork)
            {
                return;
            }
            if (Instance.consoleOutputs.Count != 0)
            {
                if (Instance.consoleOutputs.Last().output == input.ToString())
                {
                    return;

                }
            }
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.Network);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
        }

        #endregion

        Resolution _res;


        public void Awake()
        {
            // Ensure the script is not deleted while loading.
            if (!created)
            {
                DontDestroyOnLoad(this.gameObject);
                created = true;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void Start()
        {
            commands = Commands.Instance;//Instantiate commands
            WriteSystem(Utility.AwakeMessage());
            Application.logMessageReceived += new Application.LogCallback(this.PrintUnityOutput);
#if UNITY_STANDALONE
            windowRect = new Rect(200, 200, Screen.width * 1 / 2, Screen.height * 3 / 5);
#else
            windowRect = new Rect(0, 0, Screen.width, Screen.height);
#endif
        }

        public void Update()
        {
            if (_active)
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))//inputFocusTrigger gets true when user presses enter on a prediction button. But if user clicks, make this trigger false
                {
                    inputFocusTrigger = false;
                }
                OutputFilterHandler();
                OutputManager();
            }
            if (_res.height != Screen.currentResolution.height || _res.width != Screen.currentResolution.width)
            {
#if UNITY_STANDALONE
                windowRect = new Rect(windowRect.x, windowRect.y, Mathf.Clamp(windowRect.width, 140, Screen.width), Mathf.Clamp(windowRect.height, 140, Screen.height));
#else
                windowRect = new Rect(0, 0, Screen.width, Screen.height);
#endif
                _res = Screen.currentResolution;

            }
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

        private void OutputManager()//Manage console output
        {
            //Remove old logs for avoid performance issues & stackoverflow exception 
            if (consoleOutputs.Count > 100)
            {
                for (int i = 0; i < consoleOutputs.Count - 101; i++)
                {
                    consoleOutputs.Remove(consoleOutputs[i]);
                }
            }
        }

        public void SubmitInput(string _input)//Submit input string to console query
        {
            _input.Trim();
            WriteUser("} " + _input);
            QueryInput(_input);
            inputHistory.Add(_input);
            input = "";
            _historyState = -1;
            submitFocusTrigger = true;
        }


        public void QueryInput(string _input)//Make query with given input
        {

            var _inputParams = GetInputParameters(_input);
            var commandQuery = CommandQuery(_inputParams);

            if (commandQuery != null)
            {
                if (((ConsoleCommandAttribute)Attribute.GetCustomAttribute(commandQuery.GetType(), typeof(ConsoleCommandAttribute))).queryIdentity.ToLower() == _inputParams[0].ToLower())
                {
                    if (_inputParams.Length != 1)
                    {
                        var keys = commandQuery.commandParameters.Keys.ToArray();

                        for (int i = 0; i < keys.Length; i++)
                        {

                            Type genericTypeArgument = commandQuery.commandParameters[keys[i]].GetType().GenericTypeArguments[0];
                            MethodInfo method = typeof(DeveloperConsole).GetMethod("ParamQuery");
                            MethodInfo genericQuery = method.MakeGenericMethod(genericTypeArgument);
                            Type t = commandQuery.commandParameters[keys[i]].genericType.GetType();
                            if (_inputParams.Length <= i + 1)
                            {
                                WriteError("Parameter [" + keys[i] + "] is not given.");
                                return;

                            }
                            var query = genericQuery.Invoke(this, new object[] { (_inputParams[i + 1]) });

                            if (query != null)
                            {
                                commandQuery.commandParameters[keys[i]].Value = query;
                            }
                            else
                            {
                                WriteError("Parameter [" + keys[i] + "] is given wrong.");
                                return;
                            }

                        }
                        Execute(commandQuery);
                        return;
                    }
                    else if (_inputParams.Length < commandQuery.commandParameters.Keys.Count + 1)
                    {
                        WriteError("No invoke definiton for command '" + commandQuery.GetQueryIdentity() + "' takes " + (_inputParams.Length - 1) + " parameters.");
                        return;
                    }

                    Execute(commandQuery);
                    return;
                }

            }
            WriteSystem("There is no command such as '" + _input + "'");
        }

        public static Command CommandQuery(string[] inputKeywords)//Return the possible targeteted command invoke definition by user
        {
            //Invoke definitions of entered command 
            List<Command> invokeDefinitions = Commands.Instance.GetCommands().ToList().FindAll(x => x.GetQueryIdentity() == inputKeywords[0]);

            if (inputKeywords.Length == 1)
            {
                if (invokeDefinitions.Count != 0)
                {
                    foreach (Command command in invokeDefinitions)
                    {
                        if (command.commandParameters.Count == 0)
                        {
                            return command;
                        }
                    }
                    return invokeDefinitions[0];
                }
                return null;
            }
            else
            {
                foreach (Command command in invokeDefinitions)
                {
                    if (command.commandParameters.Count != 0)
                    {


                        var keys = command.commandParameters.Keys.ToList();
                        List<bool> vs = new List<bool>();
                        for (int i = 0; i < keys.Count; i++)
                        {
                            Type genericTypeArgument = command.commandParameters[keys[i]].GetType().GenericTypeArguments[0];
                            MethodInfo method = typeof(DeveloperConsole).GetMethod("ParamQuery");
                            MethodInfo genericQuery = method.MakeGenericMethod(genericTypeArgument);
                            Type t = command.commandParameters[keys[i]].genericType.GetType();
                            if (inputKeywords.Length - 1 <= i + 1)
                            {
                                //Not enough parameters for this invoke method
                                continue;
                            }
                            var query = genericQuery.Invoke(DeveloperConsole.Instance, new object[] { (inputKeywords[i + 1]) });

                            if (query != null)
                            {
                                vs.Add(true);
                            }
                            else
                            {
                                vs.Add(false);
                            }
                        }
                        if (vs.Contains(false))
                        {
                            continue;
                        }
                        return command;
                    }
                }

                try
                {
                    var firstAvailableCommand = invokeDefinitions.FindAll(x => x.commandParameters.Count != 0)[0];
                    return firstAvailableCommand;
                }
                catch
                {
                    return null;
                }


            }
        }

        public static object ParamQuery<T>(string parameter)//Make query with given parameter and type
        {
            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                Component query = null;

                var go = GameObject.Find(parameter);
                if (go != null)
                {
                    query = go.GetComponent(typeof(T));
                    if (query != null)
                    {
                        return query;

                    }
                    WriteError(parameter + " doesn't have a " + typeof(T));
                }
                return query;

            }

            else
            {
                if (Utility.TryConvert(parameter, typeof(T)))//If parameter string is convertable directly to T return converted 
                {
                    var covertedParam = (T)Convert.ChangeType(parameter, typeof(T));
                    if (covertedParam != null)
                    {
                        return covertedParam;
                    }
                }

                object query = null;
                var parameters = parameter.Split(',');
                var constructors = (typeof(T)).GetConstructors();//Get constructors of T
                ConstructorInfo _constructor = null;

                foreach (ConstructorInfo constructorInfo in constructors)//Get the possible declerations from constructors
                {
                    if (constructorInfo.GetParameters().Length == parameters.Length)
                    {
                        _constructor = constructorInfo;//Move with this decleration
                    }
                }
                if (_constructor != null)
                {
                    var constructionsParametersList = Utility.GetConstructorParametersList(_constructor, parameters);
                    if (constructionsParametersList != null)
                    {
                        query = (T)Activator.CreateInstance(typeof(T), constructionsParametersList.ToArray());
                        return query;

                    }
                    return null;
                }
                return null;
            }
        }

        public void Execute(Command command)
        {
            var output = command.Logic(); //Execute the command and get output
            if (!String.IsNullOrEmpty(output.output))
            {
                Write(output);

            }
            Instance.scrollDownTrigger = true;
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
                        _historyState = inputHistory.Count - 1;
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
            if (_active)//If active, draw console window
            {
                GUI.depth = 1;
                windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console", skin.window);
            }
            else if (drawCloseButtonOnMobile)
            {
#if UNITY_STANDALONE

#else
                if (GUI.Button(new Rect(windowRect.width - 25, 7, 15, 5), "-", skin.GetStyle("exitButton")) )
                {
                    _active = !_active;
                }
#endif
            }

        }

        void ConsoleWindow(int windowID)
        {

            printLogs = GUI.Toggle(new Rect(10, 5, 10, 10), printLogs, "", skin.GetStyle("logButton"));
            printWarnings = GUI.Toggle(new Rect(25, 5, 10, 10), printWarnings, "", skin.GetStyle("warningButton"));
            printErrors = GUI.Toggle(new Rect(40, 5, 10, 10), printErrors, "", skin.GetStyle("errorButton"));
            printNetwork = GUI.Toggle(new Rect(55, 5, 10, 10), printNetwork, "", skin.GetStyle("networkButton"));

            if (GUI.Button(new Rect(windowRect.width - 25, 7, 15, 5), "-", skin.GetStyle("exitButton")))
            {
                _active = !_active;
            }

            if (Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.KeyDown)
            {
                if (GUI.GetNameOfFocusedControl() == "consoleInputField")
                {
                    InputHistoryHandler();
                    FocusOnInputField(false);
                }
            }
#if UNITY_STANDALONE
            GUI.SetNextControlName("dragHandle");

            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));//Draw drag handle of window
#endif
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
            if (scrollDownTrigger)
            {
                scrollPosition = new Vector2(scrollPosition.x, scrollHeight);
                scrollDownTrigger = false;
            }

            GUI.SetNextControlName("textArea");
            DrawOutput();

            GUI.EndScrollView();

            GUI.SetNextControlName("submitButton");

            if (GUI.Button(new Rect(windowRect.width - 130, windowRect.height - 45, 80, 25), "Submit", skin.button))
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
            if (!String.IsNullOrEmpty(input) && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
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
                ConsoleOutputText(new Rect(20, 20 + space, windowRect.width - 60, lineSpacing), consoleOutputs[i], skin.label);
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
            if (Input.GetMouseButtonDown(0) && windowHandle.Contains(mousePos))
            {
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
            for (int i = 0; i < _input.Length; i++)
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
                if (i == _input.Length - 1 && !String.IsNullOrEmpty(_param))
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

        public void PrintUnityOutput(string outputString, string trace, LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    WriteLog(outputString);
                    return;
                case LogType.Warning:
                    WriteWarning(outputString);
                    return;
                case LogType.Error:
                    WriteError(outputString);
                    return;
                case LogType.Exception:
                    WriteError(outputString);
                    return;
                case LogType.Assert:
                    WriteWarning(outputString);
                    return;
            }
        }

    }
}