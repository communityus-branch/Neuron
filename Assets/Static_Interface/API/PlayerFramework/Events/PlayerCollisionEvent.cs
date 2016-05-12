using Static_Interface.API.EventFramework;
using UnityEngine;
using Event = Static_Interface.API.EventFramework.Event;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerCollisionEvent : Event, ICancellable
    {
        public Player Player {get;}
        public Collision Collision { get;  }
        public bool IsBullet { get; set; }
        public Vector3 Momentum { get; }
        public PlayerCollisionEvent(Player player, Collision collision, Vector3 momentum) : base(false)
        {
            Player = player;
            Collision = collision;
            Momentum = momentum;
        }

        public bool IsCancelled { get; set; }
    }
}