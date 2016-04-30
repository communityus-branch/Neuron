using System;
using System.IO;
using System.Reflection;
using Static_Interface.API.ConsoleFramework;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeatherFramework;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using Static_Interface.Neuron.Netvars;
using UnityEngine;
using Console = Static_Interface.API.ConsoleFramework.Console;

namespace Static_Interface.Internal
{
    public class World : NetworkedBehaviour
    {
        public Transform Water;
        public static World Instance;
        public static GameObject Sun_Moon => GameObject.Find("Sun_Moon");
        public GameObject Weather;
        private bool _selfDestruct;
        public Transform DefaultSpawnPosition;
        private object _commands;
        public static bool Loaded;
        protected override int PreferredChannelID => 1;

        protected override void Start ()
        {
            transform.position = new Vector3(0,0,0);
            base.Start();
            ObjectUtils.CheckObjects();
            if (Instance != null)
            {
                _selfDestruct = true;
                DestroyImmediate(this);
                return;
            }          

            Instance = this;
            Connection conn = FindObjectOfType<Connection>();
            LogUtils.Log("Initializing World...");
	        NetvarManager.Instance.RegisterNetvar(new GravityNetvar());
            NetvarManager.Instance.RegisterNetvar(new GameSpeedNetvar());
            var extensionsDir = Path.Combine(GameInfo.GameBaseDir, "Plugins");
			ExtensionManager.Init(extensionsDir);
            gameObject.AddComponent<Scheduler>();
            Weather = ObjectUtils.LoadWeather();
            var enviromentSun = GameObject.Find("__SUN__");
            var weatherParent = GameObject.Find("WeatherSystems").transform;

            var orgSunMoon = weatherParent.FindChild("Sun_Moon"); 

            ObjectUtils.CopyComponents(orgSunMoon.gameObject, enviromentSun, typeof(Light));
            enviromentSun.transform.SetParent(weatherParent);
            for (int i = 0; i < orgSunMoon.childCount; i++)
            {
                var child = orgSunMoon.GetChild(i);
                child.SetParent(enviromentSun.transform);
            }

            ObjectUtils.CopyFields(orgSunMoon.GetComponent<Light>(), enviromentSun.GetComponent<Light>());
            ObjectUtils.CopyFields(orgSunMoon.transform, enviromentSun.transform);

            Destroy(orgSunMoon.gameObject);
            enviromentSun.name = "Sun_Moon";

            var weatherSys = Weather.GetComponentInChildren<UniStormWeatherSystem_C>();
            weatherSys.sun = enviromentSun.GetComponent<Light>();

            var worldAxle = enviromentSun.transform.FindChild("WorldAxle").FindChild("WorldAxle");
            Type t = weatherSys.GetType();
            var f = t.GetField("sunComponent", BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(weatherSys, worldAxle.FindChild("Sun").GetComponent<Light>());

            var moon = worldAxle.FindChild("Moon");
            f = t.GetField("moonComponent", BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(weatherSys, moon.GetComponent<Light>());

            weatherSys.moonLight = moon.FindChild("MoonLight").GetComponent<Light>();
            gameObject.AddComponent<WeatherManager>();
            conn.SendMessage("OnWeatherInit", Weather);
            var chat = gameObject.AddComponent<Chat>();
            conn.SendMessage("OnChatInit", chat);

            if (Connection.IsClient() && !Connection.IsSinglePlayer)
            {
                _commands = new ClientConsoleCommands();
            }
            else if (Connection.IsServer())
            {
                _commands = new ServerConsoleCommands();
            }

            if(_commands != null)
                Console.Instance.RegisterCommands(_commands);
            Loaded = true;
            conn.SendMessage("OnWorldInit", this);
            var h = GetComponent<Terrain>().terrainData.size / 2;
            h.y = transform.position.y + 3000;
            Weather.transform.position = h;
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            if (_selfDestruct)
            {
                _selfDestruct = false;
                return;
            }

            Instance = null;
            if (_commands != null)
            {
                Console.Instance.UnregisterCommands(_commands);
            }
        }
    }
}
