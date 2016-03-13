using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerController : PlayerBehaviour
    {
        public float WalkSpeed = 6.0f;

        public float RunSpeed = 11.0f;
        public float JumpSpeed = 8.0f;
        public float SideSpeed = 8.0f;

        private PlayerInput _input;
        private CharacterController _controller;
        private Vector3 cachedSpeed;
        protected override void Start()
        {
            base.Start();
            _controller = GetComponent<CharacterController>();
            if (!Channel.IsOwner)
            {
                Destroy(_controller);
                _controller = null;
                //var syncer = gameObject.AddComponent<PositionSyncer>();

            }
            else if(_controller == null)
            {
                _controller = gameObject.AddComponent<CharacterController>();
                _controller.detectCollisions = true;
            }
        }

        public void UpdateInput(PlayerInput input)
        {
            _input = input;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Player.Health.IsDead || _input == null) return;
            var inputX = 0f;
            var inputY = 0f;
            bool jump = _input.GetKeyDown(KeyCode.Space);
            bool sprint = _input.GetKey(KeyCode.LeftShift);

            if (_input.GetKey(KeyCode.W))
            {
                inputY += 1;
            }
            if (_input.GetKey(KeyCode.S))
            {
                inputY -= 1;
            }
            if (_input.GetKey(KeyCode.D))
            {
                inputX += 1;
            }
            if (_input.GetKey(KeyCode.A))
            {
                inputX -= 1;
            }

            inputX *= SideSpeed;

            var y = _controller.isGrounded && jump ? JumpSpeed : 0;

            if (_controller.isGrounded)
            {
                Vector3 vel = new Vector3(inputX, y, inputY);
                var speed = sprint ? RunSpeed : WalkSpeed;
                vel.z *= speed;
                cachedSpeed = vel;
            }

            cachedSpeed -= -Physics.gravity * Time.deltaTime;
            _controller.Move(cachedSpeed*Time.deltaTime);
        }
    }
}