using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.AssetsFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using UnityEngine;
using AssetBundle = Static_Interface.API.AssetsFramework.AssetBundle;

namespace Static_Interface.API.Utils
{
    public class SpawnManager : NetworkedSingletonBehaviour<SpawnManager>
    {
        public GameObject SpawnObjectWithChannel(string assetPath, Vector3 pos, int channelId, bool init = true)
        {
            return SpawnObjectWithChannel(assetPath, pos, Quaternion.identity, channelId, init);
        }

        public GameObject SpawnObjectWithChannel(string assetPath, Vector3 pos, Quaternion rotation,
            int channelId, bool init = true)
        {
            AssetBundle bundle;
            FindAssetBundleForAsset(assetPath, out bundle);
            return SpawnObjectWithChannel(bundle, assetPath, pos, rotation, channelId, init);
        }

        public GameObject SpawnObjectWithChannel(string bundle, string assetPath, Vector3 pos, int channelId, bool init = true)
        {
            return SpawnObjectWithChannel(bundle, assetPath, pos, Quaternion.identity, channelId, init);
        }

        public GameObject SpawnObjectWithChannel(string bundle, string assetPath, Vector3 pos, Quaternion rotation,
            int channelId, bool init = true)
        {
            AssetBundle assetBundle= AssetManager.GetAssetBundle(bundle);
            return SpawnObjectWithChannel(assetBundle, assetPath, pos, rotation, channelId, init);
        }

        public GameObject SpawnObjectWithChannel(AssetBundle bundle, string assetPath, Vector3 pos, int channelId, bool init = true)
        {
            return SpawnObjectWithChannel(bundle, assetPath, pos, Quaternion.identity, channelId, init);
        }

        public GameObject SpawnObjectWithChannel(AssetBundle bundle, string assetPath, Vector3 pos, Quaternion rotation,
            int channelId, bool init = true)
        {
            var obj = SpawnObject(bundle, assetPath, pos, rotation, false);
            var ch = obj.GetComponent<Channel>();
            if (!ch)
            {
                ch = obj.AddComponent<Channel>();
            }
            ch.ID = channelId;
            ch.Setup();
            if (init)
            {
                obj = Instantiate(obj);
                Channel.Send(nameof(Networn_SpawnObjectWithChannel), ECall.Clients, bundle.Name, assetPath, pos, rotation);
            }

            return obj;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Networn_SpawnObjectWithChannel(Identity ident, string assetbundle, string assetpath, Vector3 pos, Quaternion rot, int ch)
        {
            SpawnObjectWithChannel(assetbundle, assetpath, pos, rot, ch);
        }

        public GameObject SpawnObject(string assetPath, Vector3 pos, bool init = true)
        {
            return SpawnObject(assetPath, pos, Quaternion.identity,init);
        }

        public GameObject SpawnObject(string assetPath, Vector3 pos, Quaternion rotation, bool init = true)
        {
            AssetBundle bundle;
            FindAssetBundleForAsset(assetPath, out bundle);
            return SpawnObject(bundle, assetPath, pos, rotation, init);
        }

        public GameObject SpawnObject(string bundle, string assetPath, Vector3 pos , bool init = true)
        {
            return SpawnObject(bundle, assetPath, pos, Quaternion.identity, init);
        }

        public GameObject SpawnObject(string bundle, string assetPath, Vector3 pos, Quaternion rotation, bool init = true)
        {
            AssetBundle assetBundle = AssetManager.GetAssetBundle(bundle);
            return SpawnObject(assetBundle, assetPath, pos, rotation, init);
        }

        public GameObject SpawnObject(AssetBundle bundle, string assetPath, Vector3 pos, bool init = true)
        {
            return SpawnObject(bundle, assetPath, pos, Quaternion.identity, init);
        }

        public GameObject SpawnObject(AssetBundle bundle, string assetPath, Vector3 pos, Quaternion rotation, bool init = true)
        {
            var obj = FindObject(bundle, assetPath);

            if (init)
            {
                obj = (GameObject) Instantiate(obj, pos, rotation);
                Channel.Send(nameof(Networn_SpawnObject), ECall.Clients, bundle.Name, assetPath, pos, rotation);
            }

            return obj;
        }

        private void FindAssetBundleForAsset(string assetPath, out AssetBundle bundle)
        {
            bundle = null;
            var assetName = assetPath.Split('/')[0];
            foreach (AssetBundle assetBundle in AssetManager.Bundles.Where(assetBundle => assetBundle.Contains(assetName)))
            {
                if (bundle != null) throw new Exception("Multiple asset bundles include the asset \"" + assetName + "\", please specify the bundle!");
                bundle = assetBundle;
            }

            if (bundle == null)
            {
                throw new Exception("Asset " + assetPath + " not found!");
            }
        }

        private GameObject FindObject(AssetBundle bundle, string assetPath)
        {
            var args = assetPath.Split('/');
            var obj = bundle.LoadAsset<GameObject>(args[0]);
            if (args.Length > 1)
            {
                int index = 1;
                while (true)
                {
                    if (index == args.Length - 1) break;
                    obj = obj?.transform?.FindChild(args[index])?.gameObject;
                    index++;
                }
            }

            if (obj == null)
            {
                throw new Exception("Object with path: " + assetPath + " in bundle " + bundle.Name + " not found");
            }
            return obj;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Networn_SpawnObject(Identity ident, string assetbundle, string assetpath, Vector3 pos, Quaternion rot)
        {
            SpawnObject(assetbundle, assetpath, pos, rot);
        }
    }
}