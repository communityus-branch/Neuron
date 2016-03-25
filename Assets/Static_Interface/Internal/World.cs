using System;
using System.IO;
using System.Reflection;
using Static_Interface.API.Commands;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using Static_Interface.Neuron.Netvars;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;
using Object = UnityEngine.Object;

namespace Static_Interface.Internal
{
    public class World : MonoBehaviour
    {
        public Transform Water;
        public static World Instance;
        public GameObject Weather;
        public Transform DefaultSpawnPosition;
        protected override void Start ()
        {
            base.Start();
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
            //Todo:
            if (Connection.IsServer())
            {
                LogUtils.Debug("Spawning default player");
                GameObject player =
                    (GameObject)
                        Instantiate(Resources.Load("Player"), DefaultSpawnPosition.position,
                            DefaultSpawnPosition.rotation);
                ClientConnection.SetupMainPlayer(player.transform);
            }

            var enviromentSun = GameObject.Find("__SUN__");
            var weatherParent = GameObject.Find("WeatherSystems").transform;

            var orgSunMoon = weatherParent.FindChild("Sun_Moon");
            var orgLight = orgSunMoon.GetComponent<Light>();
            ObjectUtils.CopyComponent(orgLight, enviromentSun);
            enviromentSun.transform.SetParent(weatherParent);
            for (int i = 0; i < orgSunMoon.childCount; i++)
            {
                var child = orgSunMoon.GetChild(i);
                child.SetParent(enviromentSun.transform);
            }

            enviromentSun.transform.position = orgSunMoon.transform.position;
            enviromentSun.transform.localPosition = orgSunMoon.transform.position;
            enviromentSun.transform.localEulerAngles = orgSunMoon.transform.localEulerAngles;
            enviromentSun.transform.localRotation = orgSunMoon.transform.localRotation;
            enviromentSun.transform.localScale = orgSunMoon.transform.localScale;

            Object.Destroy(orgSunMoon.gameObject);
            enviromentSun.name = "Sun_Moon";

            var weatherSys = Weather.GetComponentInChildren<UniStormWeatherSystem_C>();
            weatherSys.sun = enviromentSun.GetComponent<Light>();

            Type t = weatherSys.GetType();
            var f = t.GetField("sunComponent", BindingFlags.NonPublic);
            f.SetValue(weatherSys, enviromentSun.GetComponent<Light>());
        }
    }
}
