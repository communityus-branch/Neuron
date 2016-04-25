using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class AngleSyncer : NetworkedBehaviour
    {
        private Vector3? _cachedAngle;

        private uint _lastSync;
        public uint UpdatePeriod = 250;
        public float UpdateRadius = 250f;

        protected override void Awake()
        {
            base.Awake();
            //if (Connection.IsSinglePlayer) Destroy(this);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Connection.IsServer()) return;
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedAngle == transform.rotation.eulerAngles)
            {
                // no changes, no need for updates
                return;
            }

            _cachedAngle = transform.rotation.eulerAngles;

            Channel.Send(nameof(Network_ReadAngleServer), ECall.Server, transform.rotation.eulerAngles);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true, 
            PacketType = EPacket.UPDATE_UNRELIABLE_BUFFER)]
        protected void Network_ReadAngleServer(Identity ident, Vector3 angle)
        {
            ReadAngle(angle);
            Channel.Send(nameof(Network_ReadAngleClient), ECall.NotOwner, transform.position, angle);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true,
    PacketType = EPacket.UPDATE_UNRELIABLE_BUFFER, MaxRadius = 250f)]
        protected void Network_ReadAngleClient(Identity ident, Vector3 angle)
        {
            ReadAngle(angle);
        }

        private void ReadAngle(Vector3 angle)
        {
            transform.rotation = Quaternion.Euler(angle);
        }
    }
}