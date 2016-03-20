using System.Linq;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class AngleSyncer : NetworkedBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3? _cachedAngle;

        private uint _lastSync;
        public uint UpdatePeriod = 250;
        public float UpdateRadius = 250f;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            //if (Connection.IsSinglePlayer) Destroy(this);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Connection.IsServer()) return;
            if (TimeUtil.GetCurrentTime() - _lastSync < UpdatePeriod) return;
            if (!Channel.IsOwner) return;
            if (_cachedAngle == _rigidbody.rotation.eulerAngles)
            {
                // no changes, no need for updates
                return;
            }

            _cachedAngle = _rigidbody.rotation.eulerAngles;

            Channel.Send(nameof(ReadAngle), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, _rigidbody.rotation.eulerAngles);
            _lastSync = TimeUtil.GetCurrentTime();
        }

        [NetworkCall]
        protected void ReadAngle(Identity ident, Vector3 angle)
        {
            if (!Channel.CheckOwner(ident) && !Channel.CheckServer(ident)) return;
            if (Connection.IsServer() && ident == Channel.Connection.ServerID) return;
            _rigidbody.rotation = Quaternion.Euler(angle);
            Channel.Send(nameof(ReadAngle), ECall.NotOwner, _rigidbody.position, UpdateRadius, EPacket.UPDATE_UNRELIABLE_BUFFER, angle);
        }
    }
}