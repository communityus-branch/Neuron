using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.EventFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;

namespace Static_Interface.API.NetvarFramework
{
    public class NetvarManager : NetworkedSingletonBehaviour<NetvarManager>, IListener
    {
        private readonly List<Netvar> _netvars = new List<Netvar>();

        internal void RegisterNetvar(Netvar netvar)
        {
            LogUtils.Log("Registering Netvar: " + netvar.Name);
            if (GetNetvar(netvar.Name) != null)
            {
                throw new ArgumentException("Netvar already exists: " + netvar.Name);
            }
            _netvars.Add(netvar);
        }

        public Netvar GetNetvar(string name)
        {
            return _netvars.FirstOrDefault(tmp => tmp.Name.Equals(name));
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        public void Network_ReceiveValueUpdate(Identity ident, String name, byte[] serializedData)
        {
            Netvar netvar = GetNetvar(name);
            if (netvar == null)
            {
                if (IsClient() && !IsServer())
                {
                    throw new Exception("Netvar: " + name + " not found!");
                }
                return;
            } 
            netvar.Value = netvar.Deserialize(serializedData);
        }

        public void SendAllNetvars(Identity target)
        {
            if (!IsServer()) return;
            foreach (Netvar netvar in _netvars)
            {
                Channel.Send(nameof(Network_ReceiveValueUpdate), target, netvar.Name, netvar.Serialize());
            }
        }
    }
}