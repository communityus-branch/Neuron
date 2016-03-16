using Static_Interface.API.NetworkFramework;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerController : PlayerBehaviour
    {
        public float WalkSpeed = 6.0f;

        public float RunSpeed = 11.0f;
        public float JumpSpeed = 0.5f;
        public float SideSpeed = 8.0f;

        private PlayerInput _input;
        private CharacterController _controller;
        private Vector3 _cachedSpeed;


        // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
        public float FallingDamageThreshold = 10.0f;

        // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
        public bool SlideWhenOverSlopeLimit = false;

        // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
        public bool SlideOnTaggedObjects = false;

        public float SlideSpeed = 12.0f;

        // If checked, then the player can change direction while in the air
        public bool AirControl = false;

        // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
        public float AntiBumpFactor = .75f;

        // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
        public int AntiBunnyHopFactor = 1;
        
        private bool _grounded = false;
        private float _speed;
        private RaycastHit _hit;
        private float _fallStartLevel;
        private bool _falling;
        private float _slideLimit;
        private float _rayDistance;
        private Vector3 _contactPoint;
        private bool _playerControl = false;
        private int _jumpTimer;

        // If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster
        public bool LimitDiagonalSpeed = true;

        protected override void Start()
        {
            base.Start();
            _controller = GetComponent<CharacterController>();
            if (!Channel.IsOwner && !Connection.IsServer())
            {
                Destroy(_controller);
                _controller = null;
            }
            else if(_controller == null)
            {
                _controller = gameObject.AddComponent<CharacterController>();
                _controller.detectCollisions = true;
            }
            if (Connection.IsServer())
            {
                gameObject.AddComponent<ServerPositionSyncer>();
            }
        }

        public void UpdateInput(PlayerInput input)
        {
            _input = input;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Player?.Health == null || Player.Health.IsDead || _input == null) return;
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
                Vector3 vel = new Vector3(inputX, 0, inputY);
                var speed = sprint ? RunSpeed : WalkSpeed;
                vel.z *= speed;
                vel = transform.TransformDirection(vel);
                vel.y = y;
                _cachedSpeed = vel;
            }

            _cachedSpeed -= -Physics.gravity * Time.deltaTime;
            _controller.Move(_cachedSpeed*Time.deltaTime);
        }
    }
}