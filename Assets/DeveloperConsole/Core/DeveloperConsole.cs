﻿/*
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
        public static Color userOutputColor;
        public static Color systemOutputColor;
        public static Color logOutputColor;
        public static Color warningOutputColor;
        public static Color errorOutputColor;
        public static Color networkOutputColor;

        public Color _userOutputColor = Color.HSVToRGB(0, 0, 0.75f);
        public Color _systemOutputColor = Color.HSVToRGB(0, 0, 0.90f);
        public Color _logOutputColor = Color.HSVToRGB(0, 0, 0.90f);
        public Color _warningOutputColor = Color.yellow;
        public Color _errorOutputColor = Color.red;
        public Color _networkOutputColor = Color.cyan;

        Commands commands;//Private field for commands script
        public List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>(); //List outputs here
        private Vector2 scrollPosition = Vector2.zero;//Determine output window's scroll position
        private bool scrollDownTrigger;
        private Rect _windowRect;
        float _width;
        float _height;

        private Rect windowRect
        {
            get { return _windowRect; }
            set {

                if (windowRect.height != value.height || windowRect.width != value.width)
                {
                  OnWindowResize();
                }
                _windowRect = value;
            }
        }
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
            OnOutputChange(consoleOutput);
        }


        public static void WriteUser(object input)//Write user line without time tag
        {
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.User, false);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
            Instance.OnOutputChange(output);
        }
        public static void WriteSystem(object input)
        {
            ConsoleOutput output = new ConsoleOutput(input.ToString(), ConsoleOutput.OutputType.System);
            Instance.consoleOutputs.Add(output);
            Instance.scrollDownTrigger = true;
            Instance.OnOutputChange(output);
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
            Instance.OnOutputChange(output);
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
            Instance.OnOutputChange(output);
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
            Instance.OnOutputChange(output);
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
            Instance.OnOutputChange(output);
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
            Instance.OnOutputChange(output);
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
            userOutputColor = _userOutputColor;
            systemOutputColor = _systemOutputColor;
            logOutputColor = _logOutputColor;
            warningOutputColor = _warningOutputColor;
            errorOutputColor = _errorOutputColor;
            networkOutputColor = _networkOutputColor;
            Debug.Log(ColorUtility.ToHtmlStringRGB(networkOutputColor));
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

            if (Input.GetKeyDown(KeyCode.H))
            {
                HandleLinespacing();

            }
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

        private int outputLimit = 550; //Limit the output for performance issues
        private void OutputManager()//Manage console output
        {
            //Remove old logs for avoid performance issues & stackoverflow exception 
            if (consoleOutputs.Count > outputLimit)
            {
                for (int i = 0; i < consoleOutputs.Count - (outputLimit+1); i++)
                {
                    consoleOutputs.Remove(consoleOutputs[i]);
                }
            }
        }


        #region drawing

        void OnGUI() //GUI
        {
            GUI.Label(new Rect(0,0,50,50),textFieldHeight.ToString()) ;
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
            DrawOutput(scrollHeight);

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
                _markupOutput = "";
                WriteSystem("Flush!");
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





        public void OnWindowResize()
        {
            HandleLinespacing();
            
        }

        float textFieldHeight;

        public void OnOutputChange(ConsoleOutput output)
        {
            _markupOutput += output.output;
            textFieldHeight += CalculateConsoleOutputHeight(windowRect.width-60,output,skin.textArea);
        }

        public void HandleLinespacing()
        {
            float _fheight = 0;
            for (int i = 0; i < consoleOutputs.Count; i++)
            {
                _fheight += CalculateConsoleOutputHeight(windowRect.width - 60, consoleOutputs[i], skin.label);
           
            }
            textFieldHeight = _fheight;
        }

        string _markupOutput = ""; //collect the abstract output in this field

        private void DrawOutput(int outputHeight)
        {
            outputHeight = Mathf.Clamp(outputHeight,(int)windowRect.height,200048);
            textFieldHeight = Mathf.Clamp(textFieldHeight,20,999999);
            GUI.TextArea(new Rect(20,20,windowRect.width -40,textFieldHeight),_markupOutput,skin.textArea);
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
                    Rect _resizingWindowRect = new Rect();
                    _resizingWindowRect.width = Mathf.Clamp(_window.width + (mousePos.x - clickedPosition.x), 300, Screen.width);
                    _resizingWindowRect.height = Mathf.Clamp(_window.height + (mousePos.y - clickedPosition.y), 200, Screen.height);
                    _resizingWindowRect.y = windowRect.y;
                    _resizingWindowRect.x = windowRect.x;
                    windowRect = _resizingWindowRect;
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

        private float CalculateConsoleOutputHeight(float outputBoxWidth, ConsoleOutput consoleOutput, GUIStyle style)
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
                    style.font.GetCharacterInfo(c, out characterInfo, style.fontSize, style.fontStyle);
                    _labelWidth += (int)characterInfo.width;
                }
                lines += (int)Mathf.Clamp(Mathf.Floor(_labelWidth / outputBoxWidth), 0, 128);
            }
        
            lines += outputLines.Count() - 1;
            consoleOutput.lines = lines;
            return lines * lineSpacing;
        }

        #endregion

        #region execution

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

        #endregion

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