using System;
using Fclp;
using Static_Interface.API.ConsoleFramework;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Static_Interface.Internal.Objects;
using Static_Interface.Internal.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.Neuron.Menus
{
    [RequireComponent(typeof(AudioSource))]
    public class MainMenu : MonoBehaviour
    {
        private static bool _firstStart = true;
        private FluentCommandLineParser<ApplicationArguments> _parser;
        protected override void Awake()
        {
            base.Awake();
            CameraManager.Instance.CurrentCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            if (_firstStart)
            {
                DefaultConsoleCommands defaultCmds = new DefaultConsoleCommands();
                API.ConsoleFramework.Console.Instance.RegisterCommands(defaultCmds);
                API.ConsoleFramework.Console.Init();
                ObjectUtils.CheckObjects();
                if (!Debug.isDebugBuild && SteamAPI.RestartAppIfNecessary(GameInfo.ID))
                {
                    Application.Quit();
                    return;
                }

                Fading.Init();

                _parser = new FluentCommandLineParser<ApplicationArguments>();

                _parser.Setup(arg => arg.Dedicated)
                    .As('d', "dedicated")
                    .Callback(DedicatedCallback)
                    .SetDefault(false);

                _parser.Setup(arg => arg.Lan)
                    .As('l', "lan")
                    .SetDefault(false);

                var result = _parser.Parse(Environment.GetCommandLineArgs());
                if (result.HasErrors)
                {
                    LogUtils.LogError("Couldn't parse arguments: " + result.ErrorText);
                }

                _firstStart = false;
            }


#if !UNITY_EDITOR
            LogUtils.Debug("Commandline: " + Environment.CommandLine);

            if (Debug.isDebugBuild)
            {
                LogUtils.Debug("##### Debug build #####");
            }
#endif
        }

        private void HostDedicated()
        {
            LevelManager.Instance.InitObjects();
            AudioListener.pause = true;
            EnableConsole();
            Connection.IsDedicated = true;
            LogUtils.Log("Hosting dedicated server");
            GameObject serverObject = GameObject.Find("Server");
            ServerConnection conn = serverObject.AddComponent<ServerConnection>();
            conn.OpenGameServer();
            DontDestroyOnLoad(serverObject);
        }

        private void EnableConsole()
        {
            GameObject consoleObj = new GameObject("Console");
            consoleObj.AddComponent<ConsoleManager>();
        }

        private void DedicatedCallback(bool obj)
        {
            LogUtils.Debug("IsDedicated: " + obj);
            Connection.IsDedicated = obj;

            if (!Connection.IsDedicated)
            {
                // GetComponent<AudioSource>().Play();
            }
            else
            {
                HostDedicated();
            }
        }

        public void StartGame(string scene)
        {
            LevelManager.Instance.InitObjects();
            GameObject serverObject = GameObject.Find("Server");
            DestroyImmediate(serverObject.GetComponent<ClientConnection>());
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            DestroyImmediate(serverObject.GetComponent<SingleplayerConnection>());
            ClientConnection conn = serverObject.AddComponent<ClientConnection>();
            //SingleplayerConnection conn = serverObject.AddComponent<SingleplayerConnection>();
            //conn.Init();
            conn.AttemptConnect("127.0.0.1", 27015, string.Empty);
            DontDestroyOnLoad(serverObject);
        }

        public void Host()
        {
            LevelManager.Instance.InitObjects();
            GameObject serverObject = GameObject.Find("Server");
            DestroyImmediate(serverObject.GetComponent<ClientConnection>());
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            DestroyImmediate(serverObject.GetComponent<SingleplayerConnection>());
            //ServerConnection conn = serverObject.AddComponent<ServerConnection>();
            //conn.OpenGameServer();
            SingleplayerConnection conn = serverObject.AddComponent<SingleplayerConnection>();
            conn.Init();
            GameObject.Find("Host Button").GetComponent<Button>().enabled = false;
            DontDestroyOnLoad(serverObject);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
