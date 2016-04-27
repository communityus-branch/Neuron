using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class AngleSyncer : NetworkedBehaviour
    {
        private Quaternion? _cachedAngle;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Quaternion? _syncStartRotation;
        private Quaternion? _syncEndRotation;
        private uint _lastSync;
        public const uint UPDATE_PERIOD = 20;

        protected override void Update()
        {
            base.Update();
            _syncTime += Time.deltaTime;
            if (_syncStartRotation == null || _syncEndRotation == null) return;
            transform.rotation = Quaternion.Lerp(_syncStartRotation.Value, _syncEndRotation.Value, _syncTime / _syncDelay);
            if (transform.rotation == _syncEndRotation.Value)
            {
                _syncStartRotation = null;
                _syncEndRotation = null;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (TimeUtil.GetCurrentTime() - _lastSync < UPDATE_PERIOD) return;
            if (!Channel.IsOwner) return;
            if (_cachedAngle == transform.rotation)
            {
                // no changes, no need for updates
                return;
            }

            _cachedAngle = transform.rotation;

            Channel.Send(nameof(Network_ReadAngleServer), ECall.Server, (object) transform.rotation.eulerAngles);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        protected void Network_ReadAngleServer(Identity ident, Vector3 angle)
        {
            ReadAngle(angle, IsDedicatedServer());
            Channel.Send(nameof(Network_ReadAngleClient), ECall.NotOwner, transform.position, angle);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true, MaxRadius = 1000f)]
        protected void Network_ReadAngleClient(Identity ident, Vector3 angle, bool force)
        {
            ReadAngle(angle, false);
        }

        private void ReadAngle(Vector3 angle, bool force)
        {
            if (force)
            {
                transform.rotation = Quaternion.Euler(angle);
                return;
            }
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            _lastSync = TimeUtil.GetCurrentTime();
            _syncStartRotation = transform.rotation;
            _syncEndRotation = Quaternion.Euler(angle);
        }
    }
}