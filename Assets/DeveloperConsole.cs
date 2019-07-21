using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsole : MonoBehaviour
{


    public void Execute(Command command)
    {
        //execute here
    }

    public class Command
    {

    }


    public bool enabled = true;
    private Rect windowRect = new Rect(200, 200, Screen.width * 50 / 100, Screen.height * 60 / 100);
    public Vector2 scrollPosition = Vector2.zero;
    public int lineSpacing = 20;
    public int inputLimit = 64;

    List<ConsoleOutput> consoleOutputs = new List<ConsoleOutput>();

    public GUISkin skin;
    void OnGUI()
    {
        if (enabled)
        {
            windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console");
        }

    }
    public string input = "Command here";
    void ConsoleWindow(int windowID)
    {
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
            int scrollHeight = 0;

     
            foreach (ConsoleOutput c in consoleOutputs)
            {
                   scrollHeight += c.lines * lineSpacing;
            }
        
            scrollPosition = GUI.BeginScrollView(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), scrollPosition, new Rect(20, 20, windowRect.width - 60, scrollHeight));
            GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height * 2), "", skin.box);

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
                SmartGUITextField(new Rect(20, 20 + space, windowRect.width - 20, lineSpacing), consoleOutputs[i], skin.label);
            }
            GUI.EndScrollView();
            input = GUI.TextField(new Rect(20, windowRect.height - 45, windowRect.width - 160, 25), input, inputLimit, skin.textField);
            if (GUI.Button(new Rect(windowRect.width - 120, windowRect.height - 45, 100, 25), "Submit", skin.button))
            {
                consoleOutputs.Add(new ConsoleOutput(input));
                scrollPosition = new Vector2(scrollPosition.x, consoleOutputs.Count * 20);
            }
    }

    string SmartGUITextField(Rect position, ConsoleOutput text, GUIStyle style)
    {
        style.font.RequestCharactersInTexture(text.output, style.fontSize, style.fontStyle);
        int _labelWidth = 0;


            foreach (char c in text.output.ToCharArray())
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
        text.lines = lines;
        //Debug.Log("_labelWidth = "+ _labelWidth + "position.width = "+ position.width +" TEXT LINES " + lines);
        GUI.TextArea(new Rect(position.x,position.y,position.width, lines * 20), text.output, style);
        return null;
    }

    class ConsoleOutput
    {
        public string output;
        public int lines;

        public ConsoleOutput(string entry)
        {
            output = entry;
        }
    }
}
