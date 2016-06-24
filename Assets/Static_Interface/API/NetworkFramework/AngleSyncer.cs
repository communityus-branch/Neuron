using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class AngleSyncer : NetworkedBehaviour
    {
        public Transform TransformToSync;
        private Quaternion? _cachedAngle;
        protected override bool IsSyncable => true;
        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Quaternion? _syncStartRotation;
        private Quaternion? _syncEndRotation;

        protected override void Awake()
        {
            base.Awake();
            TransformToSync = transform;
        }

        protected override void Update()
        {
            base.Update();
            _syncTime += Time.deltaTime;
            if (_syncStartRotation == null || _syncEndRotation == null) return;
            TransformToSync.rotation = Quaternion.Lerp(_syncStartRotation.Value, _syncEndRotation.Value, _syncTime / _syncDelay);
            if (TransformToSync.rotation == _syncEndRotation.Value)
            {
                _syncStartRotation = null;
                _syncEndRotation = null;
            }
        }

        protected override bool OnSync()
        {
            base.OnSync();
            if (_cachedAngle == TransformToSync.rotation)
            {
                // no changes, no need for updates
                return false;
            }

            _cachedAngle = TransformToSync.rotation;

            Channel.Send(nameof(Network_ReadAngleServer), ECall.Server, (object)TransformToSync.rotation.eulerAngles);
            return true;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        protected void Network_ReadAngleServer(Identity ident, Vector3 angle)
        {
            if(ident != Connection.ServerID)
                ReadAngle(angle);
            Channel.Send(nameof(Network_ReadAngleClient), ECall.NotOwner, TransformToSync.position, angle);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true, MaxRadius = 1000f)]
        protected void Network_ReadAngleClient(Identity ident, Vector3 angle)
        {
            ReadAngle(angle);
        }

        private void ReadAngle(Vector3 angle)
        {
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            LastSync = TimeUtil.GetCurrentTime();
            _syncStartRotation = TransformToSync.rotation;
            _syncEndRotation = Quaternion.Euler(angle);
        }
    }
}