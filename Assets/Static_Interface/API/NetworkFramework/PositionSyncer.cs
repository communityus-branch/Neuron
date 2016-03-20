using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class PositionSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3? _cachedPosition;
        private Vector3? _cachedVelocity;

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
            //if(Connection.IsSinglePlayer) Destroy(this);
        }

        protected override void Update()
        {
            base.Update();
            if (Connection.IsSinglePlayer) return;
            _syncTime += Time.deltaTime;
            var vec = Vector3.Lerp(_syncStartPosition, _syncEndPosition, _syncTime / _syncDelay);
            if (vec == Vector3.zero) return;
            _rigidbody.position = vec;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Connection.IsServer()) return;
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedPosition == _rigidbody.position && _cachedVelocity == _rigidbody.velocity)
            {
                // no changes, no need for updates
                return;
            }

            _cachedPosition = _rigidbody.position;
            _cachedVelocity = _rigidbody.velocity;

            Channel.Send(nameof(ReadPosition), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, _rigidbody.position, _rigidbody.velocity);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall]
        protected void ReadPosition(Identity ident, Vector3 syncPosition, Vector3 syncVelocity)
        {
            if (!Channel.CheckOwner(ident) && !Channel.CheckServer(ident)) return;
            if (Connection.IsServer() && ident == Channel.Connection.ServerID) return;
            
            //Todo: check if position data is valid -> prevent speedhacks etc

            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            _lastSync = TimeUtil.GetCurrentTime();
            _syncEndPosition = syncPosition + syncVelocity*_syncDelay;
            _syncStartPosition = _rigidbody.position;
            Channel.Send(nameof(ReadPosition), ECall.NotOwner, _rigidbody.position, UpdateRadius, EPacket.UPDATE_UNRELIABLE_BUFFER, syncPosition, syncVelocity);
        }
    }
}