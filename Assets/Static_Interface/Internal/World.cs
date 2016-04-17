using System;
using System.IO;
using System.Reflection;
using Static_Interface.API.Commands;
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

namespace Static_Interface.Internal
{
    public class World : NetworkedBehaviour
    {
        public Transform Water;
        public static World Instance;
        public GameObject Sun_Moon;
        public GameObject Weather;
        private bool _selfDestruct;
        public Transform DefaultSpawnPosition;

        protected override int PreferredChannelID => 1;

        protected override void Start ()
        {
            base.Start();
            ObjectUtils.CheckObjects();
            if (Instance != null)
            {
                _selfDestruct = true;
                DestroyImmediate(this);
                return;
            }          

            Instance = this;
            LogUtils.Log("Initializing World...");
	        NetvarManager.Instance.RegisterNetvar(new GravityNetvar());
            NetvarManager.Instance.RegisterNetvar(new GameSpeedNetvar());
            new ConsoleCommands().RegisterCommands();
            var extensionsDir = Path.Combine(GameInfo.GameBaseDir, "Plugins");
			ExtensionManager.Init(extensionsDir);
            var chat = gameObject.AddComponent<Chat>();
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
            Connection conn = FindObjectOfType<Connection>();
            Sun_Moon = enviromentSun;
            gameObject.AddComponent<WeatherManager>();
            conn.SendMessage("OnPostWorldInit", chat);
        }

        protected override void OnDestroy()
        {
            if (!_selfDestruct)
            {
                Instance = null;
            }
            _selfDestruct = false;
        }
    }
}
