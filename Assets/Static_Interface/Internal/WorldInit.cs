using System;
using System.IO;
using Plugins.ConsoleUI.FrontEnd.UnityGUI;
using Static_Interface.API.Commands;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.Utils;
using Static_Interface.Neuron;
using Static_Interface.Neuron.Netvars;
using UnityEngine;

namespace Static_Interface.Internal
{
    public class WorldInit : MonoBehaviour
    {
        public Transform Water;
        public static WorldInit Instance;

        private void Start ()
        {
            ObjectUtils.CheckObjects();
            if (Instance != null) throw new Exception("Only one instance allowed");
            Instance = this;
            LogUtils.Log("Initializing World...");
	        NetvarManager.Instance.RegisterNetvar(new GravityNetvar());
            NetvarManager.Instance.RegisterNetvar(new GameSpeedNetvar());
            new ConsoleCommands().RegisterCommands();
            GameObject.Find("Console").GetComponent<ConsoleGUI>().Character = GameObject.Find("MainPlayer");
			var extensionsDir = Path.Combine(GameInfo.GameBaseDir, "Plugins");
			ExtensionManager.Init(extensionsDir);
            gameObject.AddComponent<UniStormGUIMenu>();
        }
    }
}
