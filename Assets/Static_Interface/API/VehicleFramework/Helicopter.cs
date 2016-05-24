using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.VehicleFramework
{
    public abstract class Helicopter : Vehicle
    {
        protected override Camera Camera => CameraManager.Instance.CurrentCamera; //Todo

        public GameObject MainRotor { get; protected set; }
        public GameObject TailRotor { get; protected set; }

        public float MaxMainRotorForce = 50000;
        public float MaxMainRotorVelocity = 7200;
        private float _mainRotorVelocity;
        private float _mainRotorRotation;

        public float MaxTailRotorForce = 7000;
        public float MaxTailRotorVelocity = 2200;
        private float _tailRotorVelocity;
        private float _tailRotorRotation;

        protected bool MainRotorDamaged = false;
        protected bool TailRotorDamaged = false;

        public float ForwardRotorMultiplier = 0.5f;
        public float SidewaysRotorMultiplier = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            Rigidbody.drag = 0.1f;
            Rigidbody.angularDrag = 1.5f;
            Rigidbody.mass = 2000;
        }

        //input control & torque calculations
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsDestroyed || Driver != Player.MainPlayer || Driver.Health.IsDead) return;

            Vector3 torque = new Vector3();
            Vector3 controlTorque = Vector3.zero;
            if (IsEngineStarted) controlTorque = new Vector3(PlayerController.GetInputY() * ForwardRotorMultiplier, 1.0f, -PlayerController.GetInputX() * SidewaysRotorMultiplier); 

            if (!MainRotorDamaged)
            {
                torque += (controlTorque * MaxMainRotorForce * _mainRotorVelocity);
                Rigidbody.AddRelativeForce(Vector3.up * MaxMainRotorForce * _mainRotorVelocity);
            }

            if (Vector3.Angle(Vector3.up, transform.up) < 80)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), Time.deltaTime * _mainRotorVelocity * 2);
            }

            if (!TailRotorDamaged)
            {
                torque -= (Vector3.up * MaxTailRotorForce * _tailRotorVelocity);
            }

            Rigidbody.AddRelativeTorque(torque);
        }

        protected override void Update()
        {
            if (MainRotor && !MainRotorDamaged && IsEngineStarted)
            {
                MainRotor.transform.rotation = transform.rotation *
                Quaternion.Euler(0, _mainRotorRotation, 0);
            }

            if (TailRotor && !TailRotorDamaged && IsEngineStarted)
            {
                TailRotor.transform.rotation = transform.rotation *
                Quaternion.Euler(_tailRotorRotation, 0, 0);
            }

            _mainRotorRotation += MaxMainRotorVelocity * _mainRotorVelocity * Time.deltaTime;
            _tailRotorRotation += MaxTailRotorVelocity * _mainRotorVelocity * Time.deltaTime;

            var hoverMainRotorVelocity = !MainRotor || MainRotorDamaged ? 0 : Rigidbody.mass * Physics.gravity.magnitude / MaxMainRotorForce;
            var hoverTailRotorVelocity = !TailRotor || TailRotorDamaged ? 0 : (MaxMainRotorForce * _mainRotorVelocity) / MaxTailRotorForce;

            if (MainRotor && IsEngineStarted && PlayerController.GetInputY() != 0.0 && !MainRotorDamaged)
            {
                _mainRotorVelocity += PlayerController.GetInputY() * 0.001f;
            }
            else
            {
                _mainRotorVelocity = Mathf.Lerp(_mainRotorVelocity, hoverMainRotorVelocity,
                    Time.deltaTime * Time.deltaTime * 5
                );
            }

            if (TailRotor && IsEngineStarted && !TailRotorDamaged)
            {
                _tailRotorVelocity = hoverTailRotorVelocity - PlayerController.GetInputX();
            }
            else
            {
                _tailRotorVelocity = hoverTailRotorVelocity;
            }

            if (_mainRotorVelocity > 1.0)
            {
                _mainRotorVelocity = 1.0f;
            }
            else if (_mainRotorVelocity < 0.0)
            {
                _mainRotorVelocity = 0.0f;
            }

            var audioSource = GetComponent<AudioSource>();
            if (audioSource)
                audioSource.pitch = _mainRotorVelocity;
        }

        protected override bool OnEngineStart()
        {
            if (MainRotorDamaged && TailRotorDamaged) return false;
            return true;
        }

        protected override bool OnEngineStop()
        {
            return true;
        }
    }
}