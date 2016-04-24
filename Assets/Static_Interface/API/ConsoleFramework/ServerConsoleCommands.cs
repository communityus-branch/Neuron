using Static_Interface.API.PlayerFramework;

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
            player.Health.Kill();
        }
    }
}