﻿using System;
using System.Collections;
using Static_Interface.API.EventFramework;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Static_Interface.API.LevelFramework 
{
    public class LevelManager : MonoBehaviour
    {
        public const string MENU_DIR = "Static_Interface/Neuron/Menus/";

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
            Action action = delegate 
            {
                StartCoroutine(LoadLevelInternal(level, isMenu));
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMain(action);
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
            Action action = delegate
            {
                ExtensionManager.Instance.Shutdown();
                EventManager.Instance.ClearExtensionListeners();
                NetvarManager.Instance.ClearNetvars();
                if (Connection.CurrentConnection != null)
                {
                    Connection.CurrentConnection.Dispose();
                }
                LoadLevel("MainMenu", true);
            };
            if (ThreadPool.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.QueueMain(action);
        }
    }
}