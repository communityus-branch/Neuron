using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : PlayerBehaviour
    {
        private Rigidbody _rigidbody;

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (!Channel.IsOwner)
            {
                Destroy(this);
            }
        }

        protected override void Start()
        {
            base.Start();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public float Speed = 80f;
        public float RunSpeedModifier = 1.5f;
        public float MaxVelocityChange = 10.0f;
        public bool CanJump = true;
        public float JumpHeight = 2.0f;
        private bool _grounded;
        public bool GravityEnabled = true;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsServer() && !Channel.IsOwner) return;
            if (_disabled || Player.Health.IsDead)
            {
                ApplyGravity();
                return;
            }
            var inputX = 0f;
            var inputY = 0f;
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKey(KeyCode.W))
            {
                inputY += 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputY -= 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputX += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputX -= 1;
            }

            Vector3 vel = new Vector3(inputX, 0, inputY);

            if (_grounded)
            {
                vel = transform.TransformDirection(vel);
                var speed = Speed/100*_rigidbody.mass;
                vel *= speed;
                if (sprint)
                {
                    vel *= RunSpeedModifier;
                }

                Vector3 velocity = _rigidbody.velocity;
                Vector3 velocityChange = (vel - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -MaxVelocityChange, MaxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -MaxVelocityChange, MaxVelocityChange);
                velocityChange.y = 0;
                _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

                if (CanJump && jump)
                {
                    _rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                }
            }

            ApplyGravity();
            _grounded = false;
        }

        private void ApplyGravity()
        {
            if (true) return; //Let the Rigidbody handle this
            if (!GravityEnabled) return;
            _rigidbody.AddForce(Physics.gravity * _rigidbody.mass);
        }

        protected override void OnCollisionStay (Collision collision) {
            base.OnCollisionStay(collision);
	        _grounded = true;    
	    }

        float CalculateJumpVerticalSpeed () {
	        return Mathf.Sqrt(2 * JumpHeight * -transform.InverseTransformDirection(Physics.gravity).y);
	    }

        bool _disabled;
        public void EnableControl()
        {
            if (!IsDedicatedServer() && Channel.IsOwner)
            {
                var comp = Player.GetComponent<MouseLook>();
                if (comp == null)
                {
                    comp = Player.gameObject.AddComponent<MouseLook>();
                } 
                comp.enabled = true;
            }
            _disabled = false;
        }

        public void DisableControl()
        {
            if (!IsDedicatedServer() && Channel.IsOwner)
            {
                var comp = Player.GetComponent<MouseLook>();
                Destroy(comp);
            }
            _disabled = true;
        }
    }
}