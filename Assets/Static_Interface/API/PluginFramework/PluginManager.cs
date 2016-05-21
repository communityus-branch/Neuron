using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Static_Interface.API.Utils;
using Static_Interface.PluginSandbox;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.PluginFramework
{
    //Todo: load at world load and unload when going back to MainMenu
    public class PluginManager : MonoBehaviour
    {
        protected internal override bool ForceSafeDestroy => true;
        public static PluginManager Instance { get; private set; }
        private readonly List<Plugin> _loadedPlugins = new List<Plugin>();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        private static GameObject _parentObject;

        /// <summary>
        /// Get the instance of a loaded plugin
        /// </summary>
        /// <param name="name">The name of the plugin</param>
        /// <param name="exact">If true, it will match the name case-sensitive</param>
        /// <returns>The instance of the plugin which matched, or null if no plugin was found</returns>
        public Plugin GetPlugin(string @name, bool exact = false)
        {
            foreach (Plugin pl in _loadedPlugins)
            {
                if (pl.Name.Equals(@name))
                {
                    return pl;
                }
                if (!exact && pl.Name.Trim().ToLower().Equals(@name.Trim().ToLower()))
                {
                    return pl;
                }
            }
            return null;
        }

        internal static void Init(string pluginsDir)
        {
            if (_parentObject != null) return;
            _parentObject = new GameObject("PluginManager");
            _parentObject.AddComponent<PluginManager>();
            DontDestroyOnLoad(_parentObject);
        }

        internal void Shutdown()
        {
            foreach (var pl in _loadedPlugins)
            {
                if (pl.Enabled) pl.Enabled = false;
                Assembly asm = _loadedAssemblies[pl.Path];
                Sandbox.Instance.UnloadAssembly(asm);
                _loadedAssemblies.Remove(pl.Path);
            }

            _loadedPlugins.Clear();

            Destroy(_parentObject);
            Destroy(Instance);
            _parentObject = null;
            Instance = null;
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        internal void LoadPlugins(string dir)
        {
            var files = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (string file in files.Where(file => !_loadedAssemblies.ContainsKey(file)))
            {
                LoadPlugin(file);
            }
        }

        internal void LoadPlugin(string file)
        {
            LogUtils.Debug("Loading plugin: " + Path.GetFileName(file));
            Sandbox.Instance.LoadAssembly(file, ((success, assembly, appdomain) =>
            {
                if (!success)
                {
                    LogUtils.Debug("Couldn't load plugin: " + file);
                    return;
                }
                string failedInstruction;
                string failReason;
                if (!SafeCodeHandler.IsSafeAssembly(assembly, out failedInstruction, out failReason))
                {
                    LogUtils.LogWarning("WARNING: Plugin " + file + " accesses restricted code! Check failed: " + failedInstruction + (string.IsNullOrEmpty(failReason) ? "" : " (" + failReason + ")"));
                    return;
                }
                _loadedAssemblies.Add(file, assembly);
                LoadPluginFromAssembly(assembly, appdomain, file);
            }));
        }

        internal void LoadPluginFromAssembly(Assembly asm, AppDomain domain, string path)
        {
            bool found = false;
            bool cancel = false;
            Plugin ext = null;
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Plugin))) continue;
                if (found)
                {
                    LogUtils.LogError("Assembly: " + asm + " has multiple Plugin types");
                    cancel = true;
                    break;
                };
                ext = (Plugin)domain.CreateInstanceAndUnwrap(
                    type.Assembly.FullName,
                    type.FullName);
                ext.Path = path;
                found = true;
            }

            if (!found)
            {
                LogUtils.LogError("Assembly: " + asm + " has no Plugin types (Not a plugin?)");
                return;
            }

            if (ext is GameMode)
            {
                if (GameMode.CurrentGameMode != null)
                {
                    LogUtils.LogError("Assembly: " + asm + " is registering a gamemode but a gamemode is already loaded!");
                    cancel = true;
                }
                else
                {
                    GameMode.CurrentGameMode = (GameMode) ext;
                    LogUtils.Log("Loading gamemode: " + ext.Name);
                }
            }

            if (cancel)
            {
                Sandbox.Instance.UnloadAssembly(asm);
                return;
            }

            _loadedPlugins.Add(ext);

            ext.Enabled = true;
        }

        protected override void Update()
        {
            base.Update();
            foreach (Plugin pl in _loadedPlugins.Where(ex => ex.Enabled))
            {
                pl.Update();
            }
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            Shutdown();
        }
    }
}