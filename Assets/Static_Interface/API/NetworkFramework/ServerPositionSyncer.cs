using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class ServerPositionSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _cachedPosition = Vector3.zero;
        private Vector3 _cachedVelocity = Vector3.zero;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Vector3 _syncStartPosition = Vector3.zero;
        private Vector3 _syncEndPosition = Vector3.zero;
        private uint _lastSync;
        public uint UpdatePeriod = 250;
        public float UpdateRadius = 250f;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            if(Connection.IsSinglePlayer) Destroy(this);
            if(!Connection.IsServer()) Destroy(this);
        }

        protected override void Update()
        {
            base.Update();
            _syncTime += Time.deltaTime;
            _rigidbody.position = Vector3.Lerp(_syncStartPosition, _syncEndPosition, _syncTime / _syncDelay);
        }

        protected override void FixedUpdate()
        {
            if (!Connection.IsServer()) return;
            base.FixedUpdate();
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedPosition == _rigidbody.position && _cachedVelocity == _rigidbody.velocity)
            {
                // no changes, no need for position updates
                return;
            }

            _cachedPosition = _rigidbody.position;
            _cachedVelocity = _rigidbody.velocity;

            Channel.Send(nameof(ReadPosition), ECall.Others, _rigidbody.position, UpdateRadius, EPacket.UPDATE_UNRELIABLE_BUFFER, _cachedPosition, _cachedVelocity);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall]
        protected void ReadPosition(Identity ident, Vector3 syncPosition, Vector3 syncVelocity)
        {
            if (!Channel.CheckServer(ident)) return;
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            _lastSync = TimeUtil.GetCurrentTime();
            _syncEndPosition = syncPosition + syncVelocity * _syncDelay;
            _syncStartPosition = _rigidbody.position;
        }
    }
}