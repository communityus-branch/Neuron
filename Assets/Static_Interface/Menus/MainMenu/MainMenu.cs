using System.Collections;
using Static_Interface.Level;
using Static_Interface.Multiplayer.Server;
using Static_Interface.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Static_Interface.Menus.MainMenu
{
    public class MainMenu: MonoBehaviour
    {
        void Awake()
        {
            ObjectUtils.CheckObjects();
            if (!Debug.isDebugBuild && SteamAPI.RestartAppIfNecessary(Game.ID))
            {
                Application.Quit();
            }
        }
        public void StartGame(string scene)
        {
            LevelManager.Instance.LoadLevel("DefaultMap");
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
            DestroyImmediate(serverObject.GetComponent<ServerConnection>());
            ServerConnection conn = serverObject.AddComponent<ServerConnection>();
            if (!conn.IsReady) yield return new WaitForSeconds(0.5f);
            conn.OpenGameServer();
            GameObject.Find("Host Button").GetComponent<Button>().enabled = false;
        }
    }
}
