using System.IO;
using Steamworks;
using UnityEngine;

namespace Static_Interface.The_Collapse
{
    public class GameInfo
    {
        public const string NAME = "Neuron";
        public const string VERSION = "1.0.0.0";
        public static AppId_t ID = new AppId_t(480);

        public static string GameBaseDir => Directory.GetParent(Application.dataPath).FullName;
    }
}
