using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerController : PlayerBehaviour
    {
        private CharacterController _controller;
        protected override void Start()
        {
            base.Start();
            _controller = GetComponent<CharacterController>();
            if (!Channel.IsOwner)
            {
                Destroy(_controller);
                _controller = null;
            }

            if (Connection.IsServer())
            {
                var component = gameObject.AddComponent<Rigidbody>();
                component.useGravity = false;
                component.isKinematic = true;
            }
        }
        public byte Bound { get; protected set; }

        public bool IsOnGround()
        {
            return _controller.isGrounded;
        }

        public void HandleInput(PlayerInput input)
        {
            if (Player.Health.IsDead) return;
            //todo
        }
    }
}