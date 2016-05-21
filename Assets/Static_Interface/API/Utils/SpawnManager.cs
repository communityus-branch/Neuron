using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.AssetsFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SerialisationFramework;
using UnityEngine;
using AssetBundle = Static_Interface.API.AssetsFramework.AssetBundle;

namespace Static_Interface.API.Utils
{
    public class SpawnManager : NetworkedSingletonBehaviour<SpawnManager>
    {
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

        public GameObject SpawnObject(AssetBundle bundle, string assetPath, Vector3 pos, Quaternion rotation, bool init = true, uint callback = 0)
        {
            return SpawnObjectInternal(bundle, assetPath,pos,rotation, init, callback, false);
        }

        private GameObject SpawnObjectInternal(AssetBundle bundle, string assetPath, Vector3 pos, Quaternion rotation, bool init, uint callback, bool networkCall)
        {
            if (!networkCall && !IsServer()) throw new Exception("SpawnObject can only be used from Serverside!");
            var obj = LoadObject(bundle, assetPath);

            if (init)
            {
                obj = (GameObject)Instantiate(obj, pos, rotation);
                if(!networkCall) Channel.Send(nameof(Networn_SpawnObject), ECall.Clients, bundle.Name, assetPath, pos, rotation, callback, true);
            }

            return obj;
        }
        
        private void FindAssetBundleForAsset(string assetPath, out AssetBundle bundle)
        {
            bundle = null;
            foreach (AssetBundle assetBundle in AssetManager.Bundles.Where(assetBundle => assetBundle.Contains(assetPath)))
            {
                if (bundle != null) throw new Exception("Multiple asset bundles include the asset \"" + assetPath + "\", please specify the bundle!");
                bundle = assetBundle;
            }

            if (bundle == null)
            {
                throw new Exception("Asset " + assetPath + " not found!");
            }
        }

        private GameObject LoadObject(AssetBundle bundle, string assetPath)
        {
            var obj = bundle.LoadAsset<GameObject>(assetPath);
            if (obj == null)
            {
                throw new Exception("Object with path: " + assetPath + " in bundle " + bundle.Name + " not found");
            }

            //obj has not collider, add a default one
            if (!obj.GetComponent<Collider>() && !obj.GetComponentsInChildren<Transform>().Any(t => t.GetComponent<Collider>()))
            {
                ObjectUtils.AddDefaultCollider(obj);
            }

            return obj;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Networn_SpawnObject(Identity ident, string assetbundle, string assetpath, Vector3 pos, Quaternion rot, uint callbackId, bool success)
        {
            //Prevent duplicate call of a callback when on local server
            if (Internal.MultiplayerFramework.Connection.IsSinglePlayer && ident == Connection.ClientID)
            {
                return;
            }
            List<Action<uint, bool, GameObject>> callbacks = null;
            var myIdent = Player.MainPlayer?.User?.Identity;
            if (callbackId != 0)
            {
                if (myIdent == null || !_callbacks.ContainsKey(myIdent)) return;
                callbacks = _callbacks[myIdent][callbackId];
            }
            if (!success)
            {
                if (callbacks != null)
                {
                    foreach (var t in callbacks)
                        t.Invoke(callbackId, false, null);

                    _callbacks[myIdent].Remove(callbackId);
                }
                return;
            }

            var obj = SpawnObjectInternal(AssetManager.GetAssetBundle(assetbundle), assetpath, pos, rot, true, callbackId, true);
            if (callbacks != null)
            {
                foreach (var t in callbacks)
                    t.Invoke(callbackId, true, obj);

                _callbacks[myIdent].Remove(callbackId);
            }
        }

        public void SpawnRequestClient(Action<uint, bool, GameObject> callback, AssetBundle bundle, string asset, Vector3 postion, out uint callbackId)
        {
            SpawnRequestClient(callback, bundle, asset, postion, Quaternion.identity, out callbackId);
        }

        public void SpawnRequestClient(Action<uint, bool, GameObject> callback, AssetBundle bundle, string asset, Vector3 postion, Quaternion rotation,  out uint objectId)
        {
            SpawnRequestClient(callback, bundle.Name, asset, postion, rotation, out objectId);
        }

        public void SpawnRequestClient(Action<uint, bool, GameObject> callback, string bundle, string asset, Vector3 postion, out uint callbackId)
        {
            SpawnRequestClient(callback, bundle, asset, postion, Quaternion.identity,  out callbackId);
        }


        //oh god, why??
        private readonly Dictionary<Identity, Dictionary<uint, List<Action<uint, bool, GameObject>>>> _callbacks = new Dictionary<Identity, Dictionary<uint, List<Action<uint, bool, GameObject>>>>();

        private uint _currentObjectId = 1;

        public void SpawnRequestClient(Action<uint, bool, GameObject> callback, string bundle, string asset, Vector3 position,
            Quaternion rotation, out uint objectId)
        {
            var myIdent = Player.MainPlayer.User.Identity;
            Dictionary<uint, List<Action<uint, bool, GameObject>>> myCallbacks;
            if (!_callbacks.ContainsKey(myIdent))
            {
                myCallbacks = new Dictionary<uint, List<Action<uint, bool, GameObject>>>();
                _callbacks.Add(myIdent, myCallbacks);
            }
            else
            {
                myCallbacks = _callbacks[myIdent];
            }

            if (!myCallbacks.ContainsKey(_currentObjectId))
            {
                myCallbacks.Add(_currentObjectId, new List<Action<uint, bool, GameObject>>());    
            }
            var callbacks = myCallbacks[_currentObjectId];
            callbacks.Add(callback);

            objectId = _currentObjectId;
            
            Channel.Send(nameof(Network_RequestSpawnObject), ECall.Server, bundle, asset, position, rotation, objectId);
            _currentObjectId++;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER)]
        public void Network_RequestSpawnObject(Identity ident, string bundle, string asset, Vector3 position,
            Quaternion rotation, uint callbackId)
        {
            List<Action<uint, bool, GameObject>> serverCallbacks = null;
            if (_callbacks.ContainsKey(ident) && _callbacks[ident].ContainsKey(callbackId))
            {
                serverCallbacks = _callbacks[ident][callbackId];
            }
            AssetBundle assetBundle = AssetManager.GetAssetBundle(bundle);
            ObjectSpawnRequestEvent requestEvent = new ObjectSpawnRequestEvent(bundle, asset, position, rotation, callbackId);
            requestEvent.Fire();
            if (requestEvent.IsCancelled)
            {
                Channel.Send(nameof(Networn_SpawnObject), ident, bundle, asset, position, rotation, callbackId, false);
                if (serverCallbacks != null)
                {
                    foreach (var t in serverCallbacks)
                        t.Invoke(callbackId, false, null);

                    _callbacks[ident].Remove(callbackId);
                }
                return;
            }

            var obj = SpawnObject(assetBundle, asset, position, rotation, true, callbackId);
            if (serverCallbacks != null)
            {
                foreach (var t in serverCallbacks)
                    t.Invoke(callbackId, true, obj);

                _callbacks[ident].Remove(callbackId);
            }
        }

        public uint GetNextObjectID()
        {
            return _currentObjectId;
        }

        public void AddServerCallback(Action<uint, bool, GameObject> callback, Identity target, uint callbackId)
        {
            Dictionary<uint, List<Action<uint, bool, GameObject>>> targetCallbacks;
            if (!_callbacks.ContainsKey(target))
            {
                targetCallbacks = new Dictionary<uint, List<Action<uint, bool, GameObject>>>();
                _callbacks.Add(target, targetCallbacks);
            }
            else
            {
                targetCallbacks = _callbacks[target];
            }

            if (!targetCallbacks.ContainsKey(_currentObjectId))
            {
                targetCallbacks.Add(_currentObjectId, new List<Action<uint, bool, GameObject>>());
            }
            var callbacks = targetCallbacks[_currentObjectId];
            callbacks.Add(callback);
        }
    }
}