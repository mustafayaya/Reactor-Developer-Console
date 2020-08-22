using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console {

    public interface ICommand {


        ConsoleOutput Logic();


    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandAttribute : Attribute {
        public string queryIdentity; //Like set,get,execute
        public string description;
        public bool onlyAllowedOnDeveloperVersion;

        public ConsoleCommandAttribute(string _queryIdentity, string _description)
        {
            queryIdentity = _queryIdentity;
            description = _description;
        }

        /// <summary>
        /// If true, this command will be available at only developer builds
        /// </summary>
        /// <param name="_onlyAllowedOnDeveloperVersion"></param>
        public ConsoleCommandAttribute(string _queryIdentity, string _description,bool _onlyAllowedOnDeveloperVersion)
        {
            queryIdentity = _queryIdentity;
            description = _description;
            onlyAllowedOnDeveloperVersion = _onlyAllowedOnDeveloperVersion;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CommandParameterAttribute : Attribute
    {
        public string description;
        public CommandParameter commandParameter;


        public CommandParameterAttribute(string _description)
        {
            description = _description;
        }

    }

    public class Command : ICommand
    {
        public Dictionary<string, CommandParameter> commandParameters = new Dictionary<string, CommandParameter>();

        public virtual ConsoleOutput Logic() //Logic for execute, returns the output
        {
            return new ConsoleOutput("", ConsoleOutput.OutputType.Log);
        }

        public string GetDescription()
        {
           return ((ConsoleCommandAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ConsoleCommandAttribute))).description;
        }
        public string GetQueryIdentity()
        {
            return ((ConsoleCommandAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ConsoleCommandAttribute))).queryIdentity;
        }
    }

    public abstract class CommandParameter
    {
        private object value;
        public Type genericType;
        public Command Command;//Invokable command that uses this as a parameter
        public System.Reflection.FieldInfo fieldInfo;//field name of command linked to this parameter

        public object Value
        {
            get { return value; }
            set {
                this.value = value;
                fieldInfo.SetValue(Command, value);
            }
        }
        
    }

    public class CommandParameter<TOption> : CommandParameter
    {
        public TOption Value
        {
            get {
                if (base.Value == null)
                {
                    return default;
                }
                return (TOption)base.Value;

            }
        }
        public CommandParameter(Command parentCmmand,System.Reflection.FieldInfo fieldInfo)
        {
            base.genericType = typeof(TOption);
            base.Command = parentCmmand;
            base.fieldInfo = fieldInfo;
        }

    }
    
}