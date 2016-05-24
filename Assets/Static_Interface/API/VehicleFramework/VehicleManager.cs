using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.PluginFramework;
using UnityEngine;
using AssetBundle = Static_Interface.API.AssetsFramework.AssetBundle;

namespace Static_Interface.API.VehicleFramework
{
    public class VehicleManager : NetworkedSingletonBehaviour<VehicleManager>
    {
        private readonly List<VehicleData> _vehicles = new List<VehicleData>();
        public ReadOnlyCollection<VehicleData> Vehicles => _vehicles.AsReadOnly();
         
        public void RegisterVehicle(Plugin pl, string vehicleID, AssetBundle bundle, string asset, Type controller)
        {
            if(!typeof(Vehicle).IsAssignableFrom(controller))
                throw new Exception(nameof(controller) + " has to extend " + typeof(Vehicle).FullName);

            if(!bundle.Contains(asset))
                throw new Exception($"Asset \"{asset}\" not found in bundle \"{bundle.Name}\"");

            if(GetVehicle(vehicleID) != null)
                throw new Exception($"Vehicle with ID \"{vehicleID}\" already exists!");
            VehicleData data = new VehicleData
            {
                Plugin = pl,
                ID = vehicleID,
                Bundle = bundle,
                Asset = asset,
                ControllerBehaviour = controller
            };
            _vehicles.Add(data);
        }

        public VehicleData GetVehicle(string id)
        {
            return _vehicles.FirstOrDefault(c => string.Equals(c.ID, id, StringComparison.CurrentCultureIgnoreCase));
        }

        public Vehicle SpawnVehicle(string id, Vector3 pos)
        {
            return SpawnVehicle(id, pos, Quaternion.identity);
        }

        public Vehicle SpawnVehicle(string id, Vector3 pos, Quaternion rot)
        {
            if(!IsServer()) throw new Exception("Not server");
            Channel.Send(nameof(Network_SpawnVehicle), ECall.Others, id,pos, rot);
            return SpawnVehicleInternal(id, pos, rot);
        }

        private Vehicle SpawnVehicleInternal(string id, Vector3 pos, Quaternion rot)
        {
            VehicleData data = GetVehicle(id);
            if (data == null) 
                throw new Exception($"Not Vehicle with ID \"{id}\" found!");
            GameObject veh = data.Bundle.LoadAsset<GameObject>(data.Asset);
            Vehicle vec = (Vehicle)veh.AddComponent(data.ControllerBehaviour);
            vec.ID = id;
            Instantiate(vec, pos, rot);
            return vec;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_SpawnVehicle(Identity ident, string id, Vector3 pos, Quaternion rot)
        {
            SpawnVehicleInternal(id, pos, rot);
        }

        internal void OnPluginDisabled(Plugin plugin)
        {
            var registeredVehicles = _vehicles.Where(c => c.Plugin == plugin);
            foreach (var veh in registeredVehicles.ToList())
            {
                _vehicles.Remove(veh);
            }
        }
    }

    public class VehicleData
    {
        public Plugin Plugin;
        public string ID;
        public AssetBundle Bundle;
        public string Asset;
        public Type ControllerBehaviour;
    }
}