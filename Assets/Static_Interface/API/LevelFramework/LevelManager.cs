using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Static_Interface.API.EventFramework;
using Static_Interface.API.ExtensionFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Static_Interface.API.LevelFramework 
{
    public class LevelManager : PersistentScript<LevelManager>
    {
        public const string MENU_DIR = "Static_Interface/Neuron/Menus/";

        public string CurrentLevel { get; private set; }
        public bool IsLoading { get; private set; }
        public string PendingLevel { get; private set; }

        private readonly Dictionary<Object, List<Component>> _whitelistedObjects = new Dictionary<Object, List<Component>>();

        internal void InitObjects()
        {
            foreach (Object o in FindObjectsOfType<Object>())
            {
                List<Component> list = null;
                if (o is GameObject)
                {
                    list = ((GameObject) o).GetComponents<Component>().ToList();
                }
                _whitelistedObjects.Add(o, list);
            }
        }

        internal void DestroyObjects()
        {
            foreach (Object o in FindObjectsOfType<Object>())
            {
                if (!_whitelistedObjects.ContainsKey(o))
                {
                    DestroyImmediate(o);
                }

                if (!(o is GameObject)) continue;
                var registeredComponents = _whitelistedObjects[o];
                foreach (Component comp in ((GameObject) o).GetComponents<Component>().Where(comp => !registeredComponents.Contains(comp)))
                {
                    DestroyImmediate(comp);
                }
            }
        }

        public void LoadLevel(string level, bool isMenu = false)
        {
            DestroyObjects();
            Cursor.visible = true;
            LogUtils.Log("Loading level: " + level);
            Action action = delegate 
            {
                StartCoroutine(LoadLevelInternal(level, isMenu, Connection.IsDedicated));
            };
            if (ThreadPool.Instance.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.Instance.QueueMain(action);
        }

        protected IEnumerator LoadLevelInternal(string level, bool isMenu, bool skipLoading)
        {
            IsLoading = true;
            PendingLevel = isMenu ? MENU_DIR + level : level;
            if (skipLoading)
            {
                //Todo: async support?
                SceneManager.LoadScene(PendingLevel);
                IsLoading = false;
                PendingLevel = null;
            }
            else
            {
                Fading.Instance.BeginFade(1);
                yield return new WaitForSeconds(Fading.Instance.FadeSpeed*Time.deltaTime*64);
                SceneManager.LoadSceneAsync(MENU_DIR + "LoadingMenu");
            }
        }

        protected override void OnLevelWasLoaded(int scene)
        {
            base.OnLevelWasLoaded(scene);
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

                ExtensionManager.Instance?.Shutdown();
                EventManager.Instance?.Shutdown();
                Scheduler.Instance?.Shutdown();
                NetvarManager.Instance?.Shutdown();
                Connection.CurrentConnection?.Dispose();
                Connection.CurrentConnection = null;
                LoadLevel("MainMenu", true);
            };
            if (ThreadPool.Instance.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.Instance.QueueMain(action);
        }
    }
}