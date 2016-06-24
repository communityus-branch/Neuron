using Static_Interface.API.NetworkFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PlayerBehaviour : NetworkedBehaviour
    {
        public Player Player { get; protected set; }
        public bool IsLocalPlayer => Channel.IsOwner;

        protected override void Awake()
        {
            base.Awake();
            Player = GetComponent<Player>();
        }

        public bool UseGUI()
        {
            return Channel.IsOwner && !IsDedicatedServer();
        }

        protected virtual void OnPlayerModelChange(PlayerModel newModel)
        {
            
        }
    }
}