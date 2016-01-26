using Steamworks;
using UnityEngine;

namespace Static_Interface.Internal
{
    [RequireComponent(typeof(Rigidbody))]
    public class PositionSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _cachedPosition = Vector3.zero;
        private Vector3 _cachedVelocity = Vector3.zero;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Vector3 _syncStartPosition = Vector3.zero;
        private Vector3 _syncEndPosition = Vector3.zero;

        protected override void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected override void Update()
        {
            base.Update();
            _syncTime += Time.deltaTime;
            _rigidbody.position = Vector3.Lerp(_syncStartPosition, _syncEndPosition, _syncTime / _syncDelay);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!Channel.IsOwner) return;
            if (_cachedPosition == _rigidbody.position && _cachedVelocity == _rigidbody.velocity)
            {
                // no changes, no need for position updates
                return;
            }

            _cachedPosition = _rigidbody.position;
            _cachedVelocity = _rigidbody.velocity;
            Channel.OpenWrite();
            Channel.Write(_cachedPosition, _cachedVelocity);
            Channel.CloseWrite(nameof(ReadPosition), ECall.OTHERS, EPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT);
        }

        [NetworkCall]
        protected void ReadPosition(CSteamID id)
        {
            if (!Channel.CheckOwner(id)) return;
            Vector3 syncPosition = Channel.Read<Vector3>();
            Vector3 syncVelocity = Channel.Read<Vector3>();

            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;

            _syncEndPosition = syncPosition + syncVelocity * _syncDelay;
            _syncStartPosition = _rigidbody.position;
        }
    }
}