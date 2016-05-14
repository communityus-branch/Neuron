using System;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.ConsoleFramework
{
    public class DefaultConsoleCommands
    {
        [ConsoleCommand(Runtime = ConsoleCommandRuntime.NONE)]
        [CommandUsage("nv_set <netvar> <value>")]
        [CommandHelp("Set values of netvars")]
        public void SetCommand(Netvar netvar, string args)
        {
            object value = ParseObject(args);
            if (!netvar.ValidateType(value))
            {
                Console.Instance.Print("Cannot set value: " + args);
                return;
            }

            LogUtils.Log("Setting \"" + netvar.Name + "\" to " + value);

            netvar.Value = value;
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.NONE)]
        [CommandHelp("Output current network channels")]
        public void PrintChannels()
        {
            Console.Instance.Print("Channels:");
            var channels = Object.FindObjectsOfType<Channel>();
            foreach (var ch in channels)
            {
                Console.Instance.Print("Channel #" + ch.ID + ": " + ch.gameObject.name);
            }
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.NONE)]
        [CommandHelp("Exit")]
        public void Exit()
        {
            Application.Quit();
        }

        private object ParseObject(string s)
        {
            if (s.Trim().Equals("null"))
            {
                return null;
            }

            if (s.Trim().Equals("true"))
            {
                return true;
            }

            if (s.Trim().Equals("false"))
            {
                return false;
            }

            try
            {
                return Convert.ToSingle(s.Trim());
            }
            catch (Exception)
            {

            }

            try
            {
                return Convert.ToDouble(s.Trim());
            }
            catch (Exception)
            {
                
            }

            try
            {
                var l = Convert.ToInt64(s.Trim());
                if (l <= byte.MaxValue)
                {
                    return Convert.ToByte(s.Trim());
                }

                if (l <= short.MaxValue)
                {
                    return Convert.ToInt16(s.Trim());
                }
     
                if (l <= int.MaxValue)
                {
                    return Convert.ToInt16(s.Trim());
                }

                return l;
            }
            catch (Exception)
            {
                
            }

            if (s.StartsWith("'") && s.EndsWith("'") && s.Length == 3)
            {
                return s.ToCharArray()[1];
            }

            return s;
        }
    }
}