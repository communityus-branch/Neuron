using System;
using Homans.Console;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.Commands
{
    //garbage, this will be changed in future
    public class ConsoleCommands
    {
        public void RegisterCommands()
        {
            Console.Instance.RegisterCommand("nv_set", this, nameof(SetCommand));
            Console.Instance.RegisterCommand("exit", this, nameof(Exit));
            Console.Instance.RegisterCommand("printchannels", this, nameof(PrintChannels));
            Console.Instance.RegisterCommand("revive", this, nameof(Revive));
            Console.Instance.RegisterCommand("kill", this, nameof(Kill));
            Console.Instance.RegisterParser(typeof(Netvar), ParseNetvar);
        }

        [Help("Revive yourself")]
        public void Revive()
        {
            Player.MainPlayer.Health.RevivePlayer();
        }


        [Help("Kill yourself")]
        public void Kill()
        {
            Player.MainPlayer.Health.Kill();
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

            LogUtils.Log("Setting \"" + netvar.Name + "\" to " + value);

            netvar.Value = value;
        }

        [Help("Output current network channels")]
        public void PrintChannels()
        {
            Console.Instance.Print("Channels:");
            var channels = Object.FindObjectsOfType<Channel>();
            foreach (var ch in channels)
            {
                Console.Instance.Print("Channel #" + ch.ID + ": " + ch.gameObject.name);
            }
        }
        [Help("Exit the game")]
        public void Exit()
        {
            Application.Quit();
        }

        public bool ParseNetvar(string line, out object obj)
        {
            Netvar netVar = NetvarManager.Instance.GetNetvar(line);
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