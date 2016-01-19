using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.Utils
{
    public class ObjectUtils
    {
        public static void CheckObjects()
        {
            CheckObject("PersistentScripts");
            CheckObject("SteamManager");
            CheckObject("Console");
        }

        private static void CheckObject(string name, string path = null)
        {
            if (path == null) path = name;
            if (GameObject.Find(name)) return;
            GameObject instance = Object.Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
            if(instance == null) throw new Exception("Couldn't load prefab: " + name);
            instance.name = name;
            Object.DontDestroyOnLoad(GameObject.Find(name));
        }
    }
}