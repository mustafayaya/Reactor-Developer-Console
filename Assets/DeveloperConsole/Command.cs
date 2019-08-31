using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console{

    public class Command
    {
        public string queryIdentity; //Like set,get,execute

        public Dictionary<string,CommandOption> commandOptions = new Dictionary<string, CommandOption>();

        public string description; //Desciption

        public virtual ConsoleOutput Logic() //Logic for execute, returns the output
        {
            return new ConsoleOutput("", ConsoleOutput.OutputType.Log);
        }

    }

    public abstract class CommandOption
    {
        public object optionParameter;
        public Type genericType;
    }

    public class CommandOption<TOption> : CommandOption
    {
        public TOption optionParameter;
        public CommandOption()
        {
            base.genericType = typeof(TOption);
        }

    }
    
}