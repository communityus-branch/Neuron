using System;
using Static_Interface.API.Netvar;
using Static_Interface.Commands;
using Static_Interface.Netvars;
using Static_Interface.Scripts.Netvars;
using UnityEngine;

namespace Static_Interface
{
    public class World : MonoBehaviour
    {
        public Transform water;
        public static World Instance;


        private void Start ()
        {
            if (Instance != null) throw new Exception("Only one instance allowed");
            Instance = this;
            Debug.Log("Initializing World...");
	        NetvarManager.GetInstance().RegisterNetvar(new GravityNetvar());
            NetvarManager.GetInstance().RegisterNetvar(new GameSpeedNetvar());
            new ConsoleCommands().RegisterCommands();
        }
    }
}
