using System;
using System.Collections;
using Static_Interface.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Static_Interface.Level 
{
    public class LevelManager : MonoBehaviour
    {
        public const string LevelDirs = "Static_Interface/Menus/";
        private string _currentLevel;

        public string CurrentLevel
        {
            get { return _currentLevel; }
        }

        private static LevelManager _instance;
        public static LevelManager Instance
        {
            get { return _instance; }
        }
        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        private string _pendingLevel;
        public string PendingLevel
        {
            get
            {
                return _pendingLevel;
            }
        }

        public void Start()
        {
            _instance = this;
        }

        public void LoadLevel(MonoBehaviour behaviour, string name, bool isInLevelsDir = true)
        {
            behaviour.StartCoroutine(LoadLevelInternal(name, isInLevelsDir));
        }

        protected IEnumerator LoadLevelInternal(string name, bool isInLevelsDir)
        {
            _isLoading = true;
            _pendingLevel = isInLevelsDir ? LevelDirs + name : name;
            Fading fading = GameObject.Find("PersistentScripts").GetComponent<Fading>();
            fading.BeginFade(1);
            yield return new WaitForSeconds(fading.FadeSpeed * Time.deltaTime * 64);
            SceneManager.LoadScene(LevelDirs + "LoadingMenu/LoadingMenu");
        }

        void OnLevelWasLoaded(int scene)
        {
            if (!_isLoading) return;
            if (scene == 1)
            {
                return; // Skip loading scene
            }
            _isLoading = false;
            _currentLevel = _pendingLevel;
            _pendingLevel = null;
            Debug.Log("Level has been loaded: " + _currentLevel);
        }
    }
}