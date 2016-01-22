using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using Static_Interface.Internal;
using Static_Interface.The_Collapse;
using UnityEngine;

namespace Assets.Static_Interface.API.ExtensionsFramework
{
    //Todo: load at world load and unload when going back to MainMenu
    public class ExtensionManager : MonoBehaviour
    {
        public static readonly string PLUGINS_DIR = Path.Combine(GameInfo.GameBaseDir, "Plugins");
        public static ExtensionManager Instance { get; private set; }
        private readonly List<Extension> _loadedExtensions = new List<Extension>();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(); 
        private static GameObject @object;
        public static void Init()
        {
            if (@object != null) return;
            @object = new GameObject();
            @object.AddComponent<ExtensionManager>();
            DontDestroyOnLoad(@object);
        }

        public void Shutdown()
        {
            //Todo: unload all extensions
            foreach (var extension in _loadedExtensions)
            {
                if(extension.Enabled) extension.Enabled = false;
                Assembly asm = _loadedAssemblies[extension.Path];
                Sandbox.Instance.UnloadAssembly(asm);
                _loadedAssemblies.Remove(extension.Path);
            }
            
            _loadedExtensions.Clear();

            Destroy(@object);
            Destroy(Instance);
            @object = null;
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        internal void LoadExtensions()
        {
            if (!Directory.Exists(PLUGINS_DIR))
            {
                Directory.CreateDirectory(PLUGINS_DIR);
                return;
            }

            var files = Directory.GetFiles(PLUGINS_DIR, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (string file in files.Where(file => !_loadedAssemblies.ContainsKey(file)))
            {
                Assembly asm;
                AppDomain domain;
                Sandbox.Instance.LoadAssembly(file,out asm, out domain);
                _loadedAssemblies.Add(file, asm);
                LoadExtensionFromAssembly(asm, domain, file);
            }
        }

        internal void LoadExtensionFromAssembly(Assembly asm, AppDomain domain, string path)
        {
            bool found = false;
            bool cancel = false;
            Extension ext = null;
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof (Extension))) continue;
                if (found)
                {
                    LogUtils.Error("Assembly: " + asm + " has multiple Extension types");
                    cancel = true;
                    break;
                };
                Type proxyType = typeof(Proxy);
                var proxy = (Proxy)domain.CreateInstanceAndUnwrap(
                    proxyType.Assembly.FullName,
                    proxyType.FullName);
                ext = proxy.CreateNewInstance(type);
                ext.Path = path;
                found = true;
            }

            if (!found)
            {
                LogUtils.Error("Assembly: " + asm + " has no Extension types (Not a plugin?)");
                return;
            }

            if (cancel)
            {
                Sandbox.Instance.UnloadAssembly(asm);
                return;
            }

            ext.Enabled = true;
        }

        private void Update()
        {
            foreach (Extension extension in _loadedExtensions.Where(ex => ex.Enabled))
            {
                extension.Update();
            }
        }

        private class Proxy : MarshalByRefObject
        {
            public Extension CreateNewInstance(Type t)
            {
                return (Extension)Activator.CreateInstance(t);
            }
        }
    }
}