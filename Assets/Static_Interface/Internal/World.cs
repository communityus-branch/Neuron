using System;
using System.IO;
using Static_Interface.API.Commands;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Neuron;
using Static_Interface.Neuron.Netvars;
using UnityEngine;

namespace Static_Interface.Internal
{
    public class World : MonoBehaviour
    {
        public Transform Water;
        public static World Instance;
        public GameObject Weather;
        public Transform DefaultSpawnPosition;
        private void Start ()
        {
            ObjectUtils.CheckObjects();
            if (Instance != null) throw new Exception("Only one instance allowed");
            Instance = this;
            LogUtils.Log("Initializing World...");
	        NetvarManager.Instance.RegisterNetvar(new GravityNetvar());
            NetvarManager.Instance.RegisterNetvar(new GameSpeedNetvar());
            new ConsoleCommands().RegisterCommands();
            var extensionsDir = Path.Combine(GameInfo.GameBaseDir, "Plugins");
			ExtensionManager.Init(extensionsDir);
            gameObject.AddComponent<Chat>();
            gameObject.AddComponent<Scheduler>();
            Weather = ObjectUtils.LoadWeather();
            GameObject player = (GameObject)Instantiate(Resources.Load("Player"), DefaultSpawnPosition.position, DefaultSpawnPosition.rotation);
            ClientConnection.SetupMainPlayer(player.transform);
        }
    }
}
