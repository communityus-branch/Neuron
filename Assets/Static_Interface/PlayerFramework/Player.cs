using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class Player : MonoBehaviour
    {
        public PlayerController MovementController { get; protected set; }
    }
}