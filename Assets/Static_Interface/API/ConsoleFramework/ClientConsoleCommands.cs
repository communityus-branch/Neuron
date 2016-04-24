using Static_Interface.API.PlayerFramework;

namespace Static_Interface.API.ConsoleFramework
{
    public class ClientConsoleCommands
    {
        [ConsoleCommand(Runtime = ConsoleCommandRuntime.CLIENT)]
        [CommandHelp("Revive yourself")]
        public void Revive()
        {
            Player.MainPlayer?.Health?.RevivePlayer();
        }

        [ConsoleCommand(Runtime = ConsoleCommandRuntime.CLIENT)]
        [CommandHelp("Kill yourself")]
        public void Kill()
        {
            Player.MainPlayer?.Health?.Kill();
        }
    }
}