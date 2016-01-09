using Static_Interface.Level;
using UnityEngine;

namespace Static_Interface.Menus.MainMenu
{
    public class MainMenu: MonoBehaviour
    {
        public void StartGame(string scene)
        {
            GameObject persistentScripts = GameObject.Find("PersistentScripts");
            DontDestroyOnLoad(persistentScripts);
            LevelManager.Instance.LoadLevel(this, "DefaultMap", false);
        }
    }
}
