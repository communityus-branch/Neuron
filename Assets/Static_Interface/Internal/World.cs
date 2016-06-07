using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Artngame.SKYMASTER;
using Static_Interface.API.AssetsFramework;
using Static_Interface.API.CommandFramework;
using Static_Interface.API.ConsoleFramework;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.InteractionFramework;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.VehicleFramework;
using Static_Interface.API.WeatherFramework;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron.Netvars;
using UnityEngine;
using UnityEngine.UI;
using Console = Static_Interface.API.ConsoleFramework.Console;

namespace Static_Interface.Internal
{

    public class World : NetworkedSingletonBehaviour<World>, IEntity
    {
        public Transform Water;
        public Transform DefaultSpawnPosition;
        private object _commandsObj;
        public static bool Loaded;
        protected override int PreferredChannelID => 1;
        internal object CommandsObj => _commandsObj;
        public WeatherSettings WeatherSettings;

        protected override void Start ()
        {
            base.Start();
            transform.position = new Vector3(0, 0, 0);
            InternalObjectUtils.CheckObjects();      

            LogUtils.Log("Initializing World...");
            gameObject.AddComponent<SpawnManager>();
            gameObject.AddComponent<InteractManager>();
            gameObject.AddComponent<VehicleManager>();
            var mgr = gameObject.AddComponent<NetvarManager>();
            mgr.RegisterNetvar(new GravityNetvar());
            mgr.RegisterNetvar(new GameSpeedNetvar());
            if(IsServer()) gameObject.AddComponent<CommandManager>();
            gameObject.AddComponent<Scheduler>();

            PluginManager.Init(IOUtil.GetPluginsDir());


            if (!NetworkUtils.IsDedicated())
            {
                var pausemenu = Instantiate(Resources.Load<GameObject>("UI/PauseMenu/PauseMenu"));
                var btnTransform = pausemenu.transform.FindChild("Canvas").FindChild("PauseMenuUI").FindChild("Disconnect");
                var button = btnTransform.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    LevelManager.Instance.GoToMainMenu();
                });
            }

            SkyMaster sky = gameObject.AddComponent<SkyMaster>();

            GameObject settingsObj = Resources.Load<GameObject>("WeatherSettings");

            WeatherSettings = settingsObj.GetComponent<WeatherSettings>();
            WeatherSettings.SetupSky(sky);
            WeatherSettings.SetupTerrain(GetComponent<Terrain>());
            gameObject.AddComponent<WeatherManager>();
            Connection conn = FindObjectOfType<Connection>();
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

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            if (!Directory.Exists(IOUtil.GetPluginsDir()))
            {
                Directory.CreateDirectory(IOUtil.GetPluginsDir());
            }
            LogUtils.Log("Loading plugins from dir: " + IOUtil.GetPluginsDir());

            List<string> plugins = new List<string>();
            foreach (string s in Directory.GetDirectories(IOUtil.GetPluginsDir()))
            {
                LogUtils.Debug("Plugins: Loading directory: " + s);
                string[] bundles = Directory.GetFiles(s, "*.unity3d");
                foreach(string file in bundles)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    AssetManager.LoadAssetBundle(new DirectoryInfo(s).Name + @"/" + name, file);
                }
                string pluginFile = Path.Combine(s, "Plugin.dll");
                if (File.Exists(pluginFile))
                {
                    plugins.Add(pluginFile);
                }
            }

            foreach (string file in plugins)
            {
                PluginManager.Instance.LoadPlugin(file);
            }
        }

        public string Name => "World";
    }
}
