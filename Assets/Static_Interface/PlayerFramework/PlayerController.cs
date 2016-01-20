using Static_Interface.Multiplayer;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Utils;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class PlayerController : PlayerBehaviour
    {
        private NetworkSnapshotBuffer nsb;
        private CapsuleCollider serverCollider;

        protected virtual void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (Connection.IsServer() || !Channel.IsOwner)
            {
                Destroy(_controller);
                _controller = null;
            }

            if (Connection.IsServer())
            {
                serverCollider = gameObject.AddComponent<CapsuleCollider>();
                serverCollider.isTrigger = true;
                serverCollider.center = new Vector3(0f, 1f, 0f);
                serverCollider.radius = 0.3f;
                serverCollider.height = 2f;
                serverCollider.enabled = false;
                var component = gameObject.AddComponent<Rigidbody>();
                component.useGravity = false;
                component.isKinematic = true;
                return;
            }

            nsb = new NetworkSnapshotBuffer(Connection.UPDATE_TIME, Connection.UPDATE_TIME * 2.33f);
        }

        private CharacterController _controller;
        public byte RegionX { get; protected set; }
        public byte RegionY { get; protected set; }
        public byte Bound { get; protected set; }

        public bool IsOnGround()
        {
            return _controller.isGrounded;
        }

        public void HandleInput(PlayerInput input)
        {
            if (Player.Health.IsDead) return;

        }
    }
}