using System.Collections;
using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.Server;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.Neuron.Menus
{
    [RequireComponent(typeof(AudioSource))]
    public class MainMenu: MonoBehaviour
    {
        void Awake()
        {
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
            ClientConnection conn = serverObject.AddComponent<ClientConnection>();
            //SingleplayerConnection conn = serverObject.AddComponent<SingleplayerConnection>();
            //conn.Start();
            conn.AttemptConnect("88.233.146.58", 27015, string.Empty);
        }

        public void Host()
        {
            StartCoroutine(HostCoroutine());
        }

        public void Quit()
        {
            Application.Quit();
        }

        private IEnumerator HostCoroutine()
        {
            GameObject serverObject = GameObject.Find("Server");
            DestroyImmediate(serverObject.GetComponent<ClientConnection>());
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            ServerConnection conn = serverObject.AddComponent<ServerConnection>();
            if (!conn.IsReady) yield return new WaitForSeconds(0.5f);
            conn.OpenGameServer();
            GameObject.Find("Host Button").GetComponent<Button>().enabled = false;
        }
    }
}
