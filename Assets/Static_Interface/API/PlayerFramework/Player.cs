using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class Player : MonoBehaviour
    {
        public static Player MainPlayer { get; internal set; } = null;
        public PlayerController MovementController => GetComponent<PlayerController>();
        public PlayerHealth Health => GetComponent<PlayerHealth>();
        public User User { get; internal set; }
    }
}