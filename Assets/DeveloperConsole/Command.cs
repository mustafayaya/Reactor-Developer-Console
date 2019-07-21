using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console{

    public class Command
    {
        public string queryIdentity; //Like set,get,execute
        public Dictionary<string,CommandOption> commandOptions = new Dictionary<string, CommandOption>();
        public virtual ConsoleOutput Logic() //Logic for execute, returns the output
        {
            return new ConsoleOutput("", ConsoleOutput.OutputType.Log);
        }

    }

    public class CommandOption
    {
    }

    public class CommandOption<TOption> : CommandOption 
    {
        public TOption optionParameter;

    }

}