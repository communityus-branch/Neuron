using System;
using Static_Interface.API.Netvar;
using Homans.Console;
using UnityEngine;

namespace Static_Interface.Commands
{
    public class ConsoleCommands
    {
        public void RegisterCommands()
        {
            Console.Instance.RegisterCommand("nv_set", this, "SetCommand");
            Console.Instance.RegisterCommand("exit", this, "Exit");
            Console.Instance.RegisterParser(typeof(Netvar), ParseNetvar);
        }

        [Help("Usage: nv_set <netvar> <param>\nSet values of netvars")]
        public void SetCommand(Netvar netvar, string args)
        {
            object value = ParseString(args);
            if (!netvar.ValidateType(value))
            {
                Console.Instance.Print("Cannot set value: " + args);
                return;
            }

            Console.Instance.Print("Setting \"" + netvar.Name + "\" to " + value);

            netvar.Value = value;
        }

        [Help("Exit the game")]
        public void Exit()
        {
            Application.Quit();
        }

        public bool ParseNetvar(string line, out object obj)
        {
            Netvar netVar = NetvarManager.GetInstance().GetNetvar(line);
            obj = netVar;
            return netVar != null;
        }

        public static object ParseString(string s)
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