using Static_Interface.API.Utils;
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
                return;
            }
            Cursor.visible = false;
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
        private bool _wasRigidGravityEnabled = true;
        private bool _customGravityEnabled;
        public bool CustomGravityEnabled
        {
            set
            {
                _customGravityEnabled = value;
                if (_customGravityEnabled)
                {
                    _wasRigidGravityEnabled = _rigidbody.useGravity;
                    _rigidbody.useGravity = false;
                }
                else
                {
                    _rigidbody.useGravity = _wasRigidGravityEnabled;
                }
            }
            get
            {
                return _customGravityEnabled;
            }
        }

        public Vector3 CustomGravity = Vector3.zero;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsServer() && !Channel.IsOwner) return;
            if (_disabled || Player.Health.IsDead)
            {
                ApplyGravity();
                return;
            }
            CustomGravity = Physics.gravity;
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
                var speed = Speed / 100 * _rigidbody.mass;
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
            if (!CustomGravityEnabled) return;
            _rigidbody.AddForce(CustomGravity * _rigidbody.mass);
        }

        protected override void OnCollisionStay(Collision collision)
        {
            base.OnCollisionStay(collision);
            _grounded = true;
        }

        float CalculateJumpVerticalSpeed()
        {
            return Mathf.Sqrt(2 * JumpHeight * -transform.InverseTransformDirection(Physics.gravity).y);
        }

        bool _disabled;
        public void EnableControl()
        {
            var comp = Player.GetComponent<SmoothMouseLook>();
            if (!comp)
            {
                comp = Player.gameObject.AddComponent<SmoothMouseLook>();
            }
            comp.enabled = true;
            _disabled = false;
            Cursor.visible = false;
        }

        public void DisableControl()
        {
            var comp = Player.GetComponent<SmoothMouseLook>();
            if(comp) Destroy(comp);
            _disabled = true;
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            Cursor.visible = true;
        }

#if !UNITY_EDITOR
        protected override void OnApplicationFocus(bool focusStatus)
        {
            base.OnApplicationFocus(focusStatus);
            if (focusStatus && !_disabled)
                Cursor.visible = false;
        }
#endif
    }
}