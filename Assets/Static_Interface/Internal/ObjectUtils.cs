using System;
using Static_Interface.API.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.Internal
{
    public class ObjectUtils
    {
        public static void CheckObjects()
        {
            bool created;
            var persScripts = CheckObject("PersistentScripts", out created);
            if(created) persScripts.AddComponent<ThreadPool>();
            CheckObject("SteamManager", out created);
            CheckObject("Console", out created);
        }

        private static GameObject CheckObject(string name, out bool created, string path = null)
        {
            created = false;
            if (path == null) path = name;
            var match = GameObject.Find(name);
            if (match) return match;
            created = true;
            GameObject instance = Object.Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
            if(instance == null) throw new Exception("Couldn't load prefab: " + name);
            instance.name = name;
            Object.DontDestroyOnLoad(GameObject.Find(name));
            return instance;
        }
    }
}