using System;
using System.IO;
using System.Reflection;
using Static_Interface.API.AssetsFramework;
using Static_Interface.API.ConsoleFramework;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeatherFramework;
using Static_Interface.ExtensionSandbox;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using Static_Interface.Neuron.Netvars;
using UnityEngine;
using Console = Static_Interface.API.ConsoleFramework.Console;

namespace Static_Interface.Internal
{
    public class World : NetworkedSingletonBehaviour<World>, IEntity
    {
        public Transform Water;
        public static GameObject Sun_Moon => GameObject.Find("Sun_Moon");
        public GameObject Weather;
        public Transform DefaultSpawnPosition;
        private object _commandsObj;
        public static bool Loaded;
        protected override int PreferredChannelID => 1;
        internal object CommandsObj => _commandsObj;
        protected override void Start ()
        {
            base.Start();
            transform.position = new Vector3(0, 0, 0);
            ObjectUtils.CheckObjects();      

            LogUtils.Log("Initializing World...");
	        NetvarManager.Instance.RegisterNetvar(new GravityNetvar());
            NetvarManager.Instance.RegisterNetvar(new GameSpeedNetvar());
			ExtensionManager.Init(IOUtil.GetExtensionsDir());
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
            Connection conn = FindObjectOfType<Connection>();
            conn.SendMessage("OnWeatherInit", Weather);
            var chat = gameObject.AddComponent<Chat>();
            Chat.SetInstance(chat);
            conn.SendMessage("OnChatInit", chat);

            if (Connection.IsClient() && !Connection.IsSinglePlayer)
            {
                _commandsObj = new ClientConsoleCommands();
            }
            else if (Connection.IsServer())
            {
                _commandsObj = new ServerConsoleCommands();
            }

            if(_commandsObj != null)
                Console.Instance.RegisterCommands(_commandsObj);
            Loaded = true;
            conn.SendMessage("OnWorldInit", this);
            var h = GetComponent<Terrain>().terrainData.size / 2;
            h.y = transform.position.y + 3000;
            Weather.transform.position = h;

            LoadExtensions();
        }

        private void LoadExtensions()
        {
            if (!Directory.Exists(IOUtil.GetExtensionsDir()))
            {
                Directory.CreateDirectory(IOUtil.GetExtensionsDir());
            }
            LogUtils.Log("Loading extensions from dir: " + IOUtil.GetExtensionsDir());
            foreach (string s in Directory.GetDirectories(IOUtil.GetExtensionsDir()))
            {
                LogUtils.Debug("Extensions: Loading directory: " + s);
                string[] bundles = Directory.GetFiles(s, "*.unity3d");
                foreach(string file in bundles)
                {
                    string name = Path.GetFileName(file);
                    AssetManager.LoadAsset(name, file);
                }
                string pluginFile = Path.Combine(s, "Plugin.dll");
                if (File.Exists(pluginFile))
                {
                    ExtensionManager.Instance.LoadExtension(pluginFile);
                }
            }
        }

        public string Name => "World";
    }
}
