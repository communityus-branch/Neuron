using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.Utils;

namespace Static_Interface.API.NetvarFramework
{
    public class NetvarManager
    {
        private static NetvarManager _instance;
        public static NetvarManager Instance => _instance ?? (_instance = new NetvarManager());

        private readonly List<object> _netvars = new List<object>();

        public void RegisterNetvar(Netvar netvar)
        {
            LogUtils.Log("Registering Netvar: " + netvar.Name);
            if (GetNetvar(netvar.Name) != null)
            {
                throw new ArgumentException("Netvar already exists: " + netvar.Name);
            }
            _netvars.Add(netvar);
        }

        internal void Shutdown()
        {
            _instance = null;
        }

        public Netvar GetNetvar(string name)
        {
            return _netvars.Cast<Netvar>().FirstOrDefault(tmp => tmp.Name.Equals(name));
        }
    }
}