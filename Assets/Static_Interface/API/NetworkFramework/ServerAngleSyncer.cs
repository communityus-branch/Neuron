using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class ServerAngleSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _cachedAngle = Vector3.zero;
        private Vector3 _cachedAngularVelocity = Vector3.zero;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Vector3 _syncStartAngle = Vector3.zero;
        private Vector3 _syncEndAngle = Vector3.zero;
        private uint _lastSync;
        public uint UpdatePeriod = 250;
        public float UpdateRadius = 250f;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            //if (Connection.IsSinglePlayer) Destroy(this);
            if (!Connection.IsServer()) Destroy(this);
            
        }

        protected override void Update()
        {
            base.Update();
            _syncTime += Time.deltaTime;
            _rigidbody.rotation = Quaternion.Euler(Vector3.Lerp(_syncStartAngle, _syncEndAngle, _syncTime / _syncDelay));
        }

        protected override void FixedUpdate()
        {
            if (!Connection.IsServer()) return;
            base.FixedUpdate();
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedAngle == _rigidbody.rotation.eulerAngles && _cachedAngularVelocity == _rigidbody.angularVelocity)
            {
                // no changes, no need for updates
                return;
            }

            _cachedAngle = _rigidbody.rotation.eulerAngles;
            _cachedAngularVelocity = _rigidbody.angularVelocity;

            Channel.Send(nameof(ReadAngle), ECall.Others, _rigidbody.position, UpdateRadius, EPacket.UPDATE_UNRELIABLE_BUFFER, _cachedAngle, _cachedAngularVelocity);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall]
        protected void ReadAngle(Identity ident, Vector3 angle, Vector3 angularVelocity)
        {
            if (!Channel.CheckServer(ident)) return;
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            _lastSync = TimeUtil.GetCurrentTime();
            _syncEndAngle = angle + angularVelocity * _syncDelay;
            _syncStartAngle = _rigidbody.position;
        }
    }
}