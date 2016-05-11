using System.Linq;
using Static_Interface.API.EntityFramework;
using Static_Interface.API.NetworkFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class Player : UnityExtensions.MonoBehaviour, IEntity
    {
        public static Player MainPlayer { get; internal set; } = null;
        public PlayerController MovementController => GetComponent<PlayerController>();
        public PlayerHealth Health => GetComponent<PlayerHealth>();
        public User User { get; internal set; }

        public Camera Camera => GetComponentsInChildren<Camera>().FirstOrDefault(c => c.enabled);
        public Channel Channel => GetComponent<Channel>();
        public PlayerGUI GUI => GetComponent<PlayerGUI>();
        public string Name => User.Name;
    }
}