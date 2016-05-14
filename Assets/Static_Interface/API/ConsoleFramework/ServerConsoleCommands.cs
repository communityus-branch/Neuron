using System;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.ConsoleFramework
{
    public class ServerConsoleCommands
    {
        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER)]
        [CommandHelp("Revive a player")]
        [CommandUsage("<player>")]
        public void Revive(Player player)
        {
            player.Health.RevivePlayer();
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER)]
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


        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER)]
        [CommandHelp("Kill a player")]
        [CommandUsage("<player>")]
        public void Kill(Player player)
        {
            player.Health.Kill(EDamageCause.CONSOLE);
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER)]
        [CommandHelp("Send a message to all players")]
        [CommandUsage("<text>")]
        public void Say(string text)
        {
            Chat.Instance.SendServerMessage("<b>SERVER</b>: " + text);
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER)]
        [CommandUsage("<player> <speed>")]
        public void Test(Player p, int speed)
        {
            var rigidbody = p.GetComponent<Rigidbody>();
            RigidbodyPositionSyncer posSyncer = p.GetComponentInChildren<RigidbodyPositionSyncer>();
            var ch = posSyncer.Channel;
            ch.Send("Network_ReadPositionClient", ECall.Clients, (object) rigidbody.position, Vector3.up * speed, false);
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