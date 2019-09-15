using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Console
{
    public class ConsoleOutput
    {
        public string output;
        public string dateTime;
        public OutputType outputType;

        public int lines;

        public enum OutputType
        {
            User,
            System,
            Log,
            Error,
            Warning,
            Network
        }
        public ConsoleOutput(string entry, OutputType type)
        {
            var src = DateTime.Now;
            dateTime = "(" + src.Hour + ":" + src.Minute + ":" + src.Second + ") ";
            output = entry;
            outputType = type;
        }

        public ConsoleOutput(string entry, OutputType type,bool startsWithTime)
        {
            var src = DateTime.Now;
            if (startsWithTime)
            {
                dateTime = "(" + src.Hour + ":" + src.Minute + ":" + src.Second + ") ";
            }

            output = entry;
            outputType = type;
        }

    }

    public static class Utility
    {
        public static bool GetVector3FromString(string data, out Vector3 result)
        {

            List<float> vectorCrenditicals = new List<float>();
            string crenditical = "";
            char[] _chars = data.ToCharArray();

            for (int i = 0; i < _chars.Length; i++)
            {
                if (_chars[i] != ',')
                {
                    crenditical += _chars[i];
                }
                if (_chars[i] == ',')
                {
                    float parseResult = 0;
                    if (float.TryParse(crenditical, out parseResult))
                    {
                        vectorCrenditicals.Add(parseResult);
                        crenditical = "";
                    }
                    else
                    {
                        result = Vector3.zero;
                        return false;
                    }
                }
                if (i == _chars.Length - 1)
                {
                    float parseResult = 0;
                    if (float.TryParse(crenditical, out parseResult))
                    {

                        vectorCrenditicals.Add(parseResult);
                        crenditical = "";
                    }
                }
            }
            if (vectorCrenditicals.Count == 3)
            {
                result = new Vector3(vectorCrenditicals[0], vectorCrenditicals[1], vectorCrenditicals[2]);
                return true;
            }
            else
            {

                result = Vector3.zero;
                return false;
            }
        }

        public static bool GetQuaternionFromString(string data, out Quaternion result)
        {
            List<float> vectorCrenditicals = new List<float>();
            string crenditical = "";
            char[] _chars = data.ToCharArray();

            for (int i = 0; i < _chars.Length; i++)
            {
                if (_chars[i] != ',')
                {
                    crenditical += _chars[i];
                }
                if (_chars[i] == ',')
                {
                    float parseResult = 0;
                    if (float.TryParse(crenditical, out parseResult))
                    {
                        vectorCrenditicals.Add(parseResult);
                        crenditical = "";
                    }
                    else
                    {
                        result = Quaternion.identity;
                        return false;
                    }
                }
                if (i == _chars.Length - 1)
                {
                    float parseResult = 0;
                    if (float.TryParse(crenditical, out parseResult))
                    {

                        vectorCrenditicals.Add(parseResult);
                        crenditical = "";
                    }
                }
            }
            if (vectorCrenditicals.Count ==4)
            {
                result = new Quaternion(vectorCrenditicals[0], vectorCrenditicals[1], vectorCrenditicals[2], vectorCrenditicals[2]);
                return true;
            }
            else
            {

                result = Quaternion.identity;
                return false;
            }
        }

        public static string ParamsGivenWrong<T>()
        {
            return ("Parameters for ["+ typeof(T)+"] given wrong!");
        }


       public static IEnumerable<Command> GetTypesWithCommandAttribute(System.Reflection.Assembly[] assemblies)
        {

            foreach (System.Reflection.Assembly assembly in assemblies)
            {

                foreach (System.Type type in assembly.GetTypes())
                {

                    if (type.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).Length > 0 && type.BaseType == typeof(Command))
                    {
                        Command instance = (Command)Activator.CreateInstance(type);
                        yield return instance;
                    }
                }
            }

        }

        public static List<object> GetConstructorParametersList(ConstructorInfo constructorInfo,string[] parameters)//Check each parameter in constructor and try to fill the parameters
        {
            List<object> argsList = new List<object>();
            int i = 0;

            foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(string))
                {
                    argsList.Add(parameters[i]);
                }
                if (parameterInfo.ParameterType == typeof(int))
                {
                    int result = 0;
                    if (int.TryParse(parameters[i], out result))
                    {
                        argsList.Add(result);

                    }
                    else
                    {
                        return null;
                    }
                }
                if (parameterInfo.ParameterType == typeof(float))
                {
                    float result = 0;
                    if (float.TryParse(parameters[i], out result))
                    {
                        argsList.Add(result);

                    }
                    else
                    {
                        return null;
                    }
                }

               i++;

            }
            return argsList;
        }

            public static bool TryConvert(object dummy,Type targetType)
            {
                try
                {
                  Convert.ChangeType(dummy,targetType);

                }
                catch
                {
                    return false;

                }
                return true;
            }

        public static string AwakeMessage()
        {
            
            string message = "Engine started, loaded "+System.AppDomain.CurrentDomain.GetAssemblies().Length + " assemblies, network is "+ Application.internetReachability+".";
              

            return message;
        }

        public static bool CompareLists<T>(List<T> aListA, List<T> aListB)
        {
            if (aListA == null || aListB == null || aListA.Count != aListB.Count)
                return false;
            if (aListA.Count == 0)
                return true;
            Dictionary<T, int> lookUp = new Dictionary<T, int>();
            // create index for the first list
            for (int i = 0; i < aListA.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListA[i], out count))
                {
                    lookUp.Add(aListA[i], 1);
                    continue;
                }
                lookUp[aListA[i]] = count + 1;
            }
            for (int i = 0; i < aListB.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListB[i], out count))
                {
                    // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                    return false;
                }
                count--;
                if (count <= 0)
                    lookUp.Remove(aListB[i]);
                else
                    lookUp[aListB[i]] = count;
            }
            // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
            return lookUp.Count == 0;
        }

    }

}