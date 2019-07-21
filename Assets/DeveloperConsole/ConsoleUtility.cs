using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
     public class ConsoleOutput
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
        public ConsoleOutput(string entry, OutputType type)
        {
            var src = DateTime.Now;
            string dateTimeInformation = "(" + src.Hour + ":" + src.Minute + ":" + src.Second + ") ";
            output = dateTimeInformation + entry;
            outputType = type;
        }

    }

}
