using System;
using System.Reflection;
using Static_Interface.API.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.Internal.Objects
{
    public class ObjectUtils
    {
        public static void CheckObjects()
        {
            bool created;
            var persScripts = CheckObject("PersistentScripts", out created);
            if (created)
            {
                persScripts.AddComponent<ThreadPool>();
                persScripts.AddComponent<InputUtil>();
            }
            
            CheckObject("SteamManager", out created);
            CheckObject("Console", out created);
        }

        public static GameObject LoadWeather()
        {
            GameObject weather = (GameObject) Resources.Load("Weather");
            weather = (GameObject)Object.Instantiate(weather, new Vector3(0,0,0), Quaternion.identity);
            return weather;
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

        public static Component CopyComponent(Component original, GameObject destination)
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }
    }
}