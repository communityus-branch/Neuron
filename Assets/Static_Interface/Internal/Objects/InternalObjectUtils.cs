using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.Internal.Objects
{
    public class InternalObjectUtils
    {
        public static void CheckObjects()
        {
            bool created;
            CheckObject("PersistentScripts", out created);
            
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

        public static GameObject CopyComponents(GameObject original, GameObject destination, params Type[] skips)
        {
            foreach (Component c in original.GetComponents<Component>())
            {
                Type type = c.GetType();
                if(skips.Contains(type) || type == typeof(Transform)) continue;     
                if (destination.GetComponent(type) != null)
                {
                    Object.Destroy(destination.GetComponent(type));
                }

                Component copy = destination.AddComponent(type);
                CopyFields(c, copy);
            }

            return destination;
        }

        public static T CopyFields<T>(T source, T target) 
        {
            return (T)CopyFields(source, target as object);
        }

        public static object CopyFields(object source, object target)
        {
            FieldInfo[] fields = source.GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                field.SetValue(target, field.GetValue(source));
            }

            return target;
        }

        public static void BroadcastAll(string fun, object msg)
        {
            GameObject[] gos = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
            foreach (GameObject go in gos)
            {
                if (go && go.transform.parent == null)
                {
                    go.gameObject.BroadcastMessage(fun, msg, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}