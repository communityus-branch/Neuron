using System.Collections.Generic;
using UnityEngine;

namespace Static_Interface.API.Utils
{
    public static class GameObjectExtensions
    {
        public static GameObject[] FindChildsDeep(this GameObject parent, string child)
        {
            List<GameObject> childs = new List<GameObject>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject o = parent.transform.GetChild(i).gameObject;
                if(o.name == child) childs.Add(o);
                childs.AddRange(FindChildsDeep(o, child));
            }
            return childs.ToArray();
        }

        public static GameObject FindChildPath(this GameObject parent, string path)
        {
            GameObject obj = parent;
            string[] args = path.Split('\\');
            if (args.Length == 0)
            {
                return obj.transform.Find(path)?.gameObject;
            }

            int index = 0;
            while (true)
            {
                if (index == args.Length - 1) break;
                obj = obj?.transform?.FindChild(args[index])?.gameObject;
                index++;
            }
            return obj;
        }
    }
}