using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fclp.Internals.Extensions;
using Static_Interface.API.AssetsFramework;
using Static_Interface.API.EventFramework;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Console = Static_Interface.API.ConsoleFramework.Console;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;
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
            if (_whitelistedObjects.Count > 0) return; //already initialized
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
                if (!(o is GameObject) && !(o is Component))
                {
                    Destroy(o);
                    continue;
                }

                if (!_whitelistedObjects.ContainsKey(o))
                {
                    DestroyImmediate(o);
                    continue;
                }

                if (!(o is GameObject)) continue;

                var registeredComponents = _whitelistedObjects[o];
                foreach (Component comp in ((GameObject) o).GetComponents<Component>().Where(comp => !registeredComponents.Contains(comp)))
                {
                    if (comp is MonoBehaviour)
                    {
                        ((MonoBehaviour) comp).BlockOnDestroy = true;
                    }
                    DestroyImmediate(comp);
                }
            }
        }

        public void LoadLevel(string level, bool isMenu = false)
        {
            LoadLevel(level, isMenu, false);
        }

        public void LoadLevel(string level, bool isMenu, bool keepObjects)
        {
            if(!keepObjects) DestroyObjects();
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
                return; // Skip the loading scene
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
                Unload();
                LoadLevel("MainMenu", true);
            };
            if (ThreadPool.Instance.IsMainThread)
            {
                action.Invoke();
                return;
            }
            ThreadPool.Instance.QueueMain(action);
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            Unload();
        }

        private void Unload()
        {
            if(Connection.CurrentConnection != null)
                Connection.CurrentConnection.Disconnect(null, false);
            EventManager.Instance?.Shutdown();
            GameMode.CurrentGameMode = null;
            if (World.Instance?.CommandsObj != null)
            {
                Console.Instance.UnregisterCommands(World.Instance.CommandsObj);
                AssetManager.ClearAssetBundles();
            }
        }
    }
}