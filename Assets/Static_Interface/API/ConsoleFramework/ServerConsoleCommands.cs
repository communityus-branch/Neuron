using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.PlayerFramework.ModelImpl;
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

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.SERVER, Name = "nv_set")]
        [CommandUsage("<netvar> <value>")]
        [CommandHelp("Set values of netvars")]
        public void SetCommand(Netvar netvar, string args)
        {
            object value = Console.Instance.ParseArgs(new[] {args}, new[] {typeof (Vector3)})?[0];

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
        [CommandHelp("UMATest")]
        public void UMATest(Player p)
        {
            UMAModelController cont = (UMAModelController) PlayerModelController.GetPlayerModelController("UMA");
            cont.Apply(p);
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
    }
}