using System;
using Static_Interface.API.Netvar;
using Static_Interface.Commands;
using Static_Interface.Netvars;
using Static_Interface.Scripts.Netvars;
using Assets.Plugins.ConsoleUI.FrontEnd.UnityGUI;
using Static_Interface.Utils;
using UnityEngine;

namespace Static_Interface
{
    public class World : MonoBehaviour
    {
        public Transform Water;
        public static World Instance;

        private void Start ()
        {
            ObjectUtils.CheckObjects();
            if (Instance != null) throw new Exception("Only one instance allowed");
            Instance = this;
            Debug.Log("Initializing World...");
	        NetvarManager.GetInstance().RegisterNetvar(new GravityNetvar());
            NetvarManager.GetInstance().RegisterNetvar(new GameSpeedNetvar());
            new ConsoleCommands().RegisterCommands();
            GameObject.Find("Console").GetComponent<ConsoleGUI>().Character = GameObject.Find("MainPlayer");
        }
    }
}
