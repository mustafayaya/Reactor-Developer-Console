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

    private Rect windowRect = new Rect(200, 200,Screen.width * 50/100, Screen.height * 60 / 100);
    public Vector2 scrollPosition = Vector2.zero;
    List<string> lines = new List<string>();

    public GUISkin skin;
    void OnGUI()
    {
        windowRect= GUI.Window(0, windowRect, ConsoleWindow, "Developer Console");

    }
    string Entry = "baba";
    void ConsoleWindow(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));

        scrollPosition = GUI.BeginScrollView(new Rect(20, 20, windowRect.width - 40, windowRect.height - 85), scrollPosition, new Rect(20, 20, windowRect.width - 60, lines.Count *20));
        GUI.Box(new Rect(20, 20, windowRect.width - 40, windowRect.height * 2),"",skin.box);

        for (int i = 0; i < lines.Count; i++)
        {

            SmartGUITextField(new Rect(20, 20 + i * 20, windowRect.width - 20, 60), lines[i], skin.label);
        }





        GUI.EndScrollView();
        Entry = GUI.TextField(new Rect(20, windowRect.height - 45, windowRect.width-160, 25), Entry, 64,skin.textField);
        if(GUI.Button(new Rect(windowRect.width - 120, windowRect.height - 45, 100, 25), "Submit", skin.button))
        {
            lines.Add("Presseddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd" + lines.Count);
            scrollPosition = new Vector2(scrollPosition.x, lines.Count * 20); 
        }

    }

    string SmartGUITextField(Rect position, string text,GUIStyle style)
    {
        var words = text.Split(" "[0]); //Split the string into seperate words 
        var result = "";

        for (var index = 0; index < words.Length; index++)
        {
            var word = words[index].Trim();
            if (index == 0)
            {

                result = words[0];


            }

            if (index > 0)
            {
                result += " " + word;
            }
            

            //if (TextSize.width > position.width)
            //{
            //    //remover 
            //    result = result.Substring(0, result.Length - (word.Length));
            //    result += "\n" + word;
            //    var i = Mathf.Floor( TextSize.width / position.width) * 30;
            //    GUI.TextArea(new Rect(position.x,position.y,position.width,position.height + i),text,style);
            //    return result;
            //}
        }
        GUI.TextArea(position, text, style);

        return null;
    }
}
