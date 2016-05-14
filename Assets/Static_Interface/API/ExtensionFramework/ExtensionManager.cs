using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Static_Interface.API.Utils;
using Static_Interface.ExtensionSandbox;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.ExtensionFramework
{
    //Todo: load at world load and unload when going back to MainMenu
    public class ExtensionManager : MonoBehaviour
    {
        protected internal override bool ForceSafeDestroy => true;
        public static ExtensionManager Instance { get; private set; }
        private readonly List<Extension> _loadedExtensions = new List<Extension>();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        private static GameObject _parentObject;

        /// <summary>
        /// Get the instance of a loaded extension
        /// </summary>
        /// <param name="name">The name of the extension</param>
        /// <param name="exact">If true, it will match the name case-sensitive</param>
        /// <returns>The instance of the extension which matched, or null if no extension was found</returns>
        public Extension GetExtension(string @name, bool exact = false)
        {
            foreach (Extension ext in _loadedExtensions)
            {
                if (ext.Name.Equals(@name))
                {
                    return ext;
                }
                if (!exact && ext.Name.Trim().ToLower().Equals(@name.Trim().ToLower()))
                {
                    return ext;
                }
            }
            return null;
        }

        internal static void Init(string extensionsdir)
        {
            if (_parentObject != null) return;
            _parentObject = new GameObject("ExtensionManager");
            _parentObject.AddComponent<ExtensionManager>();
            DontDestroyOnLoad(_parentObject);
        }

        internal void Shutdown()
        {
            //Todo: unload all extensions
            foreach (var extension in _loadedExtensions)
            {
                if (extension.Enabled) extension.Enabled = false;
                Assembly asm = _loadedAssemblies[extension.Path];
                Sandbox.Instance.UnloadAssembly(asm);
                _loadedAssemblies.Remove(extension.Path);
            }

            _loadedExtensions.Clear();

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

        internal void LoadExtensions(string dir)
        {
            var files = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (string file in files.Where(file => !_loadedAssemblies.ContainsKey(file)))
            {
                LoadExtension(file);
            }
        }

        internal void LoadExtension(string file)
        {
            LogUtils.Debug("Loading extension: " + Path.GetFileName(file));
            Sandbox.Instance.LoadAssembly(file, ((success, assembly, appdomain) =>
            {
                if (!success)
                {
                    LogUtils.Debug("Couldn't load extension: " + file);
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
                LoadExtensionFromAssembly(assembly, appdomain, file);
            }));
        }

        internal void LoadExtensionFromAssembly(Assembly asm, AppDomain domain, string path)
        {
            bool found = false;
            bool cancel = false;
            Extension ext = null;
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Extension))) continue;
                if (found)
                {
                    LogUtils.LogError("Assembly: " + asm + " has multiple Extension types");
                    cancel = true;
                    break;
                };
                ext = (Extension)domain.CreateInstanceAndUnwrap(
                    type.Assembly.FullName,
                    type.FullName);
                ext.Path = path;
                found = true;
            }

            if (!found)
            {
                LogUtils.LogError("Assembly: " + asm + " has no Extension types (Not a plugin?)");
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

            _loadedExtensions.Add(ext);

            ext.Enabled = true;
        }

        protected override void Update()
        {
            base.Update();
            foreach (Extension extension in _loadedExtensions.Where(ex => ex.Enabled))
            {
                extension.Update();
            }
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            Shutdown();
        }
    }
}