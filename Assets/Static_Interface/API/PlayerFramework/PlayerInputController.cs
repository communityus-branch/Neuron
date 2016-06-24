using Static_Interface.API.ConsoleFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeaponFramework;
using Static_Interface.Internal;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerInputController : PlayerBehaviour
    {
        private Rigidbody _rigidbody;
        private ListenerBehaviour _listener;

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
            CheckRigidBody(Player.Model.gameObject);
            var obj = Player.Model.gameObject;
            AddRigidbodySyncer(obj);
            _listener = obj.AddComponent<ListenerBehaviour>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void AddRigidbodySyncer(GameObject obj)
        {
            obj.SetActive(false);
            var syncer = obj.AddComponent<RigidbodyPositionSyncer>();
            syncer.Channel = Channel;
            obj.SetActive(true);
        }

        public float Speed = 80f;
        public float RunSpeedModifier = 1.5f;
        public float MaxVelocityChange = 10.0f;
        public bool CanJump = true;
        public float JumpHeight = 1.0f;

        public static float GetInputX()
        {
            var inputX = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                inputX += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputX -= 1;
            }
            return inputX;
        }

        public static float GetInputY()
        {
            var inputY = 0f;
            if (Input.GetKey(KeyCode.W))
            {
                inputY += 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputY -= 1;
            }
            return inputY;
        }

        public static float GetInputZ()
        {
            var inputZ = 0f;
            if (Input.GetKey(KeyCode.Space))
            {
                inputZ += 1;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                inputZ -= 1;
            }
            return inputZ;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsServer() && !Channel.IsOwner) return;
            if (_disabled || Player.Health.IsDead)
            {
                return;
            }

            if (ConsoleGUI.Instance.IsOpen) return;
            if (PauseHook.IsPaused) return;
            var inputX = GetInputX();
            var inputY = GetInputY();

            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift);
            
            Vector3 vel = new Vector3(inputX, 0, inputY);

            if (_listener.Grounded)
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

            _listener.Grounded = false;
        }

        protected override void OnPlayerModelChange(PlayerModel newModel)
        {
            _listener = newModel.Model.AddComponent<ListenerBehaviour>();
            CheckRigidBody(newModel.Model);
            AddRigidbodySyncer(newModel.Model);
        }

        private void CheckRigidBody(GameObject model)
        {
            _rigidbody = model.GetComponent<Rigidbody>();
            if (!_rigidbody)
            {
                _rigidbody = model.AddComponent<Rigidbody>();
                _rigidbody.mass = 80;
                _rigidbody.freezeRotation = true;
            }
        }

        float CalculateJumpVerticalSpeed()
        {
            return Mathf.Sqrt(2 * JumpHeight * -transform.InverseTransformDirection(Physics.gravity).y);
        }

        bool _disabled;
        public void EnableControl()
        {
            var mouse = Player.GetComponent<PlayerMouseLook>();
            if(!mouse)
                mouse = Player.gameObject.AddComponent<PlayerMouseLook>();
            mouse.enabled = !ConsoleGUI.Instance.IsOpen;
            _disabled = false;
            Cursor.visible = false;
            GetComponent<WeaponController>().enabled = true;
        }

        public void DisableControl()
        {
            var comp = Player.GetComponent<PlayerMouseLook>();
            if(comp) Destroy(comp);
            GetComponent<WeaponController>().enabled = false;
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

        private class ListenerBehaviour : MonoBehaviour
        {
            public bool Grounded;
            protected override void OnCollisionStay(Collision collision)
            {
                base.OnCollisionStay(collision);
                Grounded = true;
            }
        }
    }
}