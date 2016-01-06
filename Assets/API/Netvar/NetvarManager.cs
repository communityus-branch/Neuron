using System;
using System.Collections.Generic;
using System.Linq;
using Assets.API.EventFramework;
using Assets.Events;
using UnityEngine;

namespace Assets.API.Netvar
{
    public class NetvarManager
    {
        private static NetvarManager _instance;
        private readonly List<object> _netvars = new List<object>();
        public NetvarManager()
        {
            EventManager.GetInstance().RegisterEvents(this);
        }

        public static NetvarManager GetInstance()
        {
            return _instance ?? (_instance = new NetvarManager());
        }

        public void RegisterNetvar(Netvar netvar)
        {
            Debug.Log("Registering Netvar: " + netvar.Name);
            if (GetNetvar(netvar.Name) != null)
            {
                throw new ArgumentException("Netvar already exists: " + netvar.Name);
            }
            _netvars.Add(netvar);
        }

        public void ClearNetvars()
        {
            //Todo
        }

        public Netvar GetNetvar(string name)
        {
            return _netvars.Cast<Netvar>().FirstOrDefault(tmp => tmp.Name.Equals(name));
        }

        [EventFramework.EventHandler(Priority = EventPriority.MONITOR)]
        public void OnNetvarChange<T>(NetvarChangedEvent @event)
        {
            Debug.Log("Netvar \"" + @event.Name + "\" updated:" + @event.OldValue + " -> " + @event.NewValue);
        }
    }
}