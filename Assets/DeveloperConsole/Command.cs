using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console{

    public class Command
    {
        public string queryIdentity; //Like set,get,execute

        public virtual ConsoleOutput Logic() //Logic for execute, returns the output
        {
            return new ConsoleOutput("", ConsoleOutput.OutputType.Log);
        }

    }


    public class CommandOptions<T>
    {
        T options;
    }

}