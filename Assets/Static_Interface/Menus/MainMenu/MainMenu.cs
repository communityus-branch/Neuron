using System.Collections;
using Static_Interface.Level;
using Static_Interface.Multiplayer.Server;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.Menus.MainMenu
{
    public class MainMenu: MonoBehaviour
    {
        void Awake()
        {
            if (!Debug.isDebugBuild && SteamAPI.RestartAppIfNecessary(Game.ID))
            {
                Application.Quit();
            }
        }
        public void StartGame(string scene)
        {
            DontDestroyOnLoad(GameObject.Find("PersistentScripts"));
            DontDestroyOnLoad(GameObject.Find("Server"));
            LevelManager.Instance.LoadLevel("DefaultMap");
        }

        public void Host()
        {
            StartCoroutine(HostCoroutine());
        }


        private IEnumerator HostCoroutine()
        {
            GameObject serverObject = GameObject.Find("Server");
            ServerConnection conn = serverObject.GetComponent<ServerConnection>() ??
                                    serverObject.AddComponent<ServerConnection>();
            if (!conn.IsReady) yield return new WaitForSeconds(0.5f);
            conn.OpenGameServer();
            GameObject.Find("Host Button").GetComponent<Button>().enabled = false;
        }
    }
}
