using Assets.API.Netvar;
using Assets.ConsoleUI.Commands;
using Assets.Netvars;
using UnityEngine;

namespace Assets
{
    public class Init : MonoBehaviour {

        // Use this for initialization
        void Start () {
            Debug.Log("Initializing...");
	        NetvarManager.GetInstance().RegisterNetvar(new GravityNetvar());
            new Commands().RegisterCommands();
        }
	
        // Update is called once per frame
        void Update () {
	
        }
    }
}
