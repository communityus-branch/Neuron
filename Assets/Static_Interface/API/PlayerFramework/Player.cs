﻿using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class Player : MonoBehaviour
    {
        public PlayerController MovementController => GetComponent<PlayerController>();
        public PlayerHealth Health => GetComponent<PlayerHealth>();
        public User User { get; internal set; }
    }
}