using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Static_Interface.API.Utils;
using Static_Interface.ExtensionSandbox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.AssetsFramework
{
    public class AssetBundle
    {
        private readonly UnityEngine.AssetBundle _assetBundle;
        public readonly string Name;
        private readonly string _assetFile;
        private readonly List<Object> _loadedObjects = new List<Object>();
        public ReadOnlyCollection<Object> LoadedObjects => _loadedObjects.AsReadOnly();
        public string FilePath => _assetFile;

        private GameObject _bundleScripts;
        internal AssetBundle(string name, string file) : this(name, file, 0U)
        {
        }

        internal AssetBundle(string name, string file, uint crc)
        {
            _assetFile = file;
            _assetBundle = UnityEngine.AssetBundle.LoadFromFile(file, crc);
            Name = name;
        }

        public Component[] LoadAllScripts()
        {
            var components = new List<Component>();
            foreach (string s in GetAllAssetNames())
            {
                try
                {
                    if (s.EndsWith("Script") || s.EndsWith("Scripts"))
                    {
                        components.AddRange(LoadScripts(s));
                    }
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }
            return components.ToArray();
        }

        public Component[] LoadScripts(string assetName, Type t = null)
        {
            TextAsset asset = null;
            try
            {
                asset = _assetBundle.LoadAsset<TextAsset>(assetName);
            }
            catch (Exception)
            {
                // ignored
            }

            if (asset == null)
            {
                LogUtils.Debug("No scripts found in asset bundle: " + _assetBundle);
                return new Component[0];
            }

            if(_bundleScripts == null) _bundleScripts = new GameObject(Name + "-BundleScripts");
            List<Component> components = new List<Component>();
            var assembly = Assembly.Load(asset.bytes);
            string failedAt;
            string failedInstruction;
            if (!SafeCodeHandler.IsSafeAssembly(assembly, out failedInstruction, out failedAt))
            {
                _assetBundle.Unload(true);
                throw new Exception("Illegal asset script detected: [" + failedInstruction + "] at " + failedAt + " is not allowed (file: " + _assetFile + ")");
            }

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof (Component)) && type != typeof (Component)) continue;
                if(t != null && (!type.IsSubclassOf(t) && type != t)) continue;
                AttachToGameObjectAttribute[] attrs = (AttachToGameObjectAttribute[]) type.GetCustomAttributes(typeof (AttachToGameObjectAttribute),
                    true);

                bool wasAttached = false;
                foreach (AttachToGameObjectAttribute attr in attrs)
                {
                    GameObject obj = LoadAsset<Object>(attr.GameObject) as GameObject;
                    if (obj == null)
                    {
                        LogUtils.LogError("AttachToGameObjectAttribute: Couldn't find GameObject \"" + attr.GameObject + "\" in component " + type.FullName + " in bundle " + _assetBundle + "!");
                        continue;
                    }

                    components.Add(_bundleScripts.AddComponent(type));
                    wasAttached = true;
                }

                if (!wasAttached)
                {
                    components.Add(_bundleScripts.AddComponent(type));
                }
            }

            return components.ToArray();
        }

        public string[] GetAllAssetNames()
        {
            return _assetBundle.GetAllAssetNames();
        }

        private T GetLoadedObject<T>(string s) where T : Object
        {
            return (T) _loadedObjects.FirstOrDefault(o => o.name == s);
        }

        public T LoadAsset<T>(string name, bool forceNewInstance = false) where T : Object
        {
            if (!forceNewInstance)
            {
                var loadedObj = GetLoadedObject<T>(name);
                if (loadedObj)
                {
                    return loadedObj;
                }
            }

            return _assetBundle.LoadAsset<T>(name);
        }

        public bool Contains(string asset)
        {
            return _assetBundle.Contains(asset);
        }

        public T[] LoadAllAssets<T>() where T : Object
        {
            return _assetBundle.LoadAllAssets<T>();
        }

        public AssetBundleRequest LoadAssetAsync(string name)
        {
            return _assetBundle.LoadAssetAsync(name);
        }

        public AssetBundleRequest LoadAllAssetAsync()
        {
            return _assetBundle.LoadAllAssetsAsync();
        }

        public void Unload()
        {
            _assetBundle.Unload(false);
        }
    }
}