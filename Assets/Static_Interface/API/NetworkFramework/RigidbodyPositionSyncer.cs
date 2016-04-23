using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyPositionSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3? _cachedPosition;
        private Vector3? _cachedVelocity;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Vector3? _syncStartPosition;
        private Vector3? _syncEndPosition;
        private uint _lastSync;
        public uint UpdatePeriod = 250;
        public float UpdateRadius = 250f;
        public IPositionValidator PositionValidator;
        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            //if(Connection.IsSinglePlayer) Destroy(this);
        }

        protected override void Update()
        {
            base.Update();
            if (Channel.IsOwner) return;
            _syncTime += Time.deltaTime;
            if (_syncStartPosition == null || _syncEndPosition == null) return;
            var vec = Vector3.Lerp(_syncStartPosition.Value, _syncEndPosition.Value, _syncTime / _syncDelay);
            _rigidbody.position = vec;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedPosition == _rigidbody.position && _cachedVelocity == _rigidbody.velocity)
            {
                // no changes, no need for updates
                return;
            }

            _cachedPosition = _rigidbody.position;
            _cachedVelocity = _rigidbody.velocity;
            Channel.Send(nameof(Network_ReadPosition), ECall.Server, _rigidbody.position, _rigidbody.velocity);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.BOTH, ValidateServer = true, ValidateOwner = true)]
        protected void Network_ReadPosition(Identity ident, Vector3 syncPosition, Vector3 syncVelocity)
        {
            if (Connection.IsServer() && ident == Channel.Connection.ServerID) return;

            if (Connection.IsServer() && PositionValidator != null)
            {
                var deltaPosition = syncPosition - _rigidbody.position;
                var deltaVelocity = syncVelocity - _rigidbody.velocity;
                if (!PositionValidator.ValidatePosition(_rigidbody.transform, deltaPosition, deltaVelocity))
                {
                    Channel.Send(nameof(Network_ReadPosition), ECall.Owner, _rigidbody.position, _rigidbody.velocity);
                    return;
                }
            }

            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            _lastSync = TimeUtil.GetCurrentTime();
            _syncEndPosition = syncPosition + syncVelocity*_syncDelay;
            _syncStartPosition = _rigidbody.position;
            Channel.Send(nameof(Network_ReadPosition), ECall.NotOwner, _rigidbody.position, UpdateRadius, syncPosition, syncVelocity);
        }
    }
}