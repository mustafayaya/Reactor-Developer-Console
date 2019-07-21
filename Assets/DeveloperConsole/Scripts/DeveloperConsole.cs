using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsole : MonoBehaviour
{

    [Header("Console Settings")]
    public bool active = true;
    public GUISkin skin;
    public int lineSpacing = 20;
    public int inputLimit = 64;
    public Color logOutputColor = Color.white;
    public Color warningOutputColor = Color.yellow;
    public Color errorOutputColor = Color.red;
    public Color networkOutputColor = Color.cyan;



    List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>();
    private Vector2 scrollPosition = Vector2.zero;
    private Rect windowRect = new Rect(200, 200, Screen.width * 50 / 100, Screen.height * 60 / 100);
    string input = "Command here";

    public void Execute(Command command)
    {
        //execute here
    }

    public class Command
    {

    }


    void OnGUI()
    {
        if (active)
        {
            windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console");
        }

    }
    void ConsoleWindow(int windowID)
    {
        GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), "", skin.box);

        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
            int scrollHeight = 0;

     
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
            input = GUI.TextField(new Rect(20, windowRect.height - 45, windowRect.width - 160, 25), input, inputLimit, skin.textField);
            if (GUI.Button(new Rect(windowRect.width - 130, windowRect.height - 45, 80, 25), "Submit", skin.button))
            {
                consoleOutputs.Add(new ConsoleOutput(input,ConsoleOutput.OutputType.Log));

            scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
            }
             if (GUI.Button(new Rect(windowRect.width - 40, windowRect.height - 45, 20, 25), "X", skin.button))
             {
                consoleOutputs.Clear();
                scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
             }
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

        int lines = (int)Mathf.Clamp(Mathf.Floor( _labelWidth / position.width),1,128) ;
        if (lines != 1)
        {
            lines += 2;
        }
        consoleOutput.lines = lines;
        switch (consoleOutput.outputType){
            case ConsoleOutput.OutputType.Log:
                GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = logOutputColor } });
                break;
            case ConsoleOutput.OutputType.Warning:
                GUI.TextArea(new Rect(position.x, position.y, position.width, lines * 20), consoleOutput.output, new GUIStyle(style) { normal = new GUIStyleState() { textColor = warningOutputColor} });
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

    class ConsoleOutput
    {
        public string output;
        public OutputType outputType;

        public int lines;

        public enum OutputType
        {
            Log,
            Error,
            Warning,
            Network
        }
        public ConsoleOutput(string entry,OutputType type)
        {
            output = entry;
            outputType = type;

        }
    }
}
