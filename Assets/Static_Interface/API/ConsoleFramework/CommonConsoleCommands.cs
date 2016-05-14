using Static_Interface.API.NetworkFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.ConsoleFramework
{
    public class DefaultConsoleCommands
    {
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
    }
}