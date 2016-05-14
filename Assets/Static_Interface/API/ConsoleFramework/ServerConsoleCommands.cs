using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
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
    }
}