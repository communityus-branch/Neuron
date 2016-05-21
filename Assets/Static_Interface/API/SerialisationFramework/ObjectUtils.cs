using System.Collections.Generic;
using UnityEngine;

namespace Static_Interface.API.SerialisationFramework
{
    public static class ObjectUtils
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

        public static void AddDefaultCollider(GameObject obj)
        {
            var newCollider = obj.gameObject.AddComponent<MeshCollider>();
            newCollider.convex = true;
            newCollider.sharedMesh = GetCombinedMesh(obj);
        }

        public static Mesh GetCombinedMesh(GameObject o)
        {
            MeshFilter[] meshFilters = o.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }
            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }
    }
}