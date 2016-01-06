using Assets.API.Netvar;
using Assets.ConsoleUI.Commands;
using System;
using Assets.Netvars;
using UnityEngine;

namespace Assets
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
            new Commands().RegisterCommands();
        }
    }
}
