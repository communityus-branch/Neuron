using System.Collections;
using Static_Interface.API.Netvar;
using Static_Interface.API.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Static_Interface.API.Level 
{
    public class LevelManager : MonoBehaviour
    {
        public const string MENU_DIR = "Static_Interface/The_Collapse/Menus/";

        public string CurrentLevel { get; private set; }
        public static LevelManager Instance { get; private set; }
        public bool IsLoading { get; private set; }
        public string PendingLevel { get; private set; }

        public void Start()
        {
            Instance = this;
        }

        public void LoadLevel(string level, bool isMenu = false)
        {
            StartCoroutine(LoadLevelInternal(level, isMenu));
        }

        protected IEnumerator LoadLevelInternal(string level, bool isMenu)
        {
            IsLoading = true;
            PendingLevel = isMenu ? MENU_DIR + level : level;
            Fading fading = GameObject.Find("PersistentScripts").GetComponent<Fading>();
            fading.BeginFade(1);
            yield return new WaitForSeconds(fading.FadeSpeed * Time.deltaTime * 64);
            SceneManager.LoadScene(MENU_DIR + "LoadingMenu");
        }

        void OnLevelWasLoaded(int scene)
        {
            if (!IsLoading) return;
            if (scene == 1)
            {
                return; // Skip loading scene
            }
            IsLoading = false;
            CurrentLevel = PendingLevel;
            PendingLevel = null;
            LogUtils.Log("Level has been loaded: " + CurrentLevel);
        }

        public void GoToMainMenu()
        {
            NetvarManager.Instance.ClearNetvars();
            LoadLevel("MainMenu", true);
        }
    }
}