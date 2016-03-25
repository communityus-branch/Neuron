using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Static_Interface.Internal.Objects;
using UnityEngine;
using UnityEngine.UI;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.Neuron.Menus
{
    [RequireComponent(typeof(AudioSource))]
    public class MainMenu: MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            ObjectUtils.CheckObjects();
            //if (!Debug.isDebugBuild && SteamAPI.RestartAppIfNecessary(GameInfo.ID))
            //{
            //    Application.Quit();
            //}
            GetComponent<AudioSource>().Play();
        }

        public void StartGame(string scene)
        {
            GameObject serverObject = GameObject.Find("Server");
            DestroyImmediate(serverObject.GetComponent<ClientConnection>());
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            DestroyImmediate(serverObject.GetComponent<SingleplayerConnection>());
            ClientConnection conn = serverObject.AddComponent<ClientConnection>();
            //SingleplayerConnection conn = serverObject.AddComponent<SingleplayerConnection>();
            //conn.Init();
            conn.AttemptConnect("88.223.87.145", 27015, string.Empty);
        }

        public void Host()
        {
            GameObject serverObject = GameObject.Find("Server");
            DestroyImmediate(serverObject.GetComponent<ClientConnection>());
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            //ServerConnection conn = serverObject.AddComponent<ServerConnection>();
            //conn.OpenGameServer();
            SingleplayerConnection conn = serverObject.AddComponent<SingleplayerConnection>();
            conn.Init();
            GameObject.Find("Host Button").GetComponent<Button>().enabled = false;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
