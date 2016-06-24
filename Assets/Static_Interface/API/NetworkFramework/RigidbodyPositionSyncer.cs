using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class RigidbodyPositionSyncer : NetworkedBehaviour
    {
        public Rigidbody RigidbodyToSync;
        private Vector3? _cachedPosition;
        private Vector3? _cachedVelocity;

        private float _lastSynchronizationTime;
        private float _syncDelay;
        private float _syncTime;
        private Vector3? _syncStartPosition;
        private Vector3? _syncEndPosition;

        protected override bool IsSyncable => true;

        private readonly List<IPositionValidator> _positionValidators = new List<IPositionValidator>();

        public ReadOnlyCollection<IPositionValidator> PositionValidators => _positionValidators.AsReadOnly();

        public void AddPositionValidator(IPositionValidator validator)
        {
            _positionValidators.Add(validator);
        }

        public void RemovePositionValidator(IPositionValidator validator)
        {
            _positionValidators.Remove(validator);
        }

        protected override void Awake()
        {
            base.Awake();
            RigidbodyToSync = GetComponent<Rigidbody>();
        }

        /*
        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();
            if (!Channel.IsOwner && !IsServer())
            {
                Rigidbody.useGravity = false;
            }
        }
        */

        protected override void Update()
        {
            base.Update();
            if (RigidbodyToSync == null || !RigidbodyToSync) return;
            _syncTime += Time.deltaTime;
            if (_syncStartPosition == null || _syncEndPosition == null) return;
            var vec = Vector3.Lerp(_syncStartPosition.Value, _syncEndPosition.Value, _syncTime / _syncDelay);
            RigidbodyToSync.position = vec;

            if (vec == _syncEndPosition.Value)
            {
                _syncStartPosition = null;
                _syncEndPosition = null;
            }
        }

        protected override bool OnSync()
        {
            base.OnSync();
            if (RigidbodyToSync == null || !RigidbodyToSync) return false;
            if (_cachedPosition == RigidbodyToSync.position && _cachedVelocity == RigidbodyToSync.velocity)
            {
                // no changes, no need for updates
                return false;
            }

            _cachedPosition = RigidbodyToSync.position;
            _cachedVelocity = RigidbodyToSync.velocity;
            Channel.Send(nameof(Network_ReadPositionServer), ECall.Server, (object)RigidbodyToSync.position, RigidbodyToSync.velocity);
            return true;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        protected void Network_ReadPositionServer(Identity ident, Vector3 syncPosition, Vector3 syncVelocity)
        {
            if (RigidbodyToSync == null || !RigidbodyToSync) return;
            if (ident != Connection.ServerID)
            {
                if (_positionValidators.Count > 0)
                {
                    var deltaPosition = syncPosition - RigidbodyToSync.position;
                    var deltaVelocity = syncVelocity - RigidbodyToSync.velocity;
                    if (_positionValidators.Any(val => !val.ValidatePosition(ident, RigidbodyToSync.transform, deltaPosition, deltaVelocity)))
                    {
                        Channel.Send(nameof(Network_ReadPositionClient), ECall.Owner, (object)RigidbodyToSync.position,
                            RigidbodyToSync.velocity, true);
                        return;
                    }
                }

                ReadPosition(syncPosition, syncVelocity, IsDedicatedServer());
            }

            Channel.Send(nameof(Network_ReadPositionClient), ECall.NotOwner, RigidbodyToSync.position, syncPosition, syncVelocity, false);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer= true, MaxRadius = 1000)]
        protected void Network_ReadPositionClient(Identity ident, Vector3 syncPosition, Vector3 syncVelocity, bool force)
        {
            if (RigidbodyToSync == null || !RigidbodyToSync) return;
            ReadPosition(syncPosition, syncVelocity, force);
        }

        private void ReadPosition(Vector3 syncPosition, Vector3 syncVelocity, bool snap)
        {
            if (snap)
            {
                RigidbodyToSync.position = syncPosition;
                RigidbodyToSync.velocity = syncVelocity;
                return;
            }
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            LastSync = TimeUtil.GetCurrentTime();
            _syncEndPosition = syncPosition + syncVelocity * _syncDelay;
            _syncStartPosition = RigidbodyToSync.position;
        }

        public static void AddRigidbodySyncer(GameObject obj, Rigidbody body, Channel ch)
        {
            obj.SetActive(false);
            var syncer = obj.AddComponent<RigidbodyPositionSyncer>();
            if (body != null) syncer.RigidbodyToSync = body;
            if (ch != null) syncer.Channel = ch;
            obj.SetActive(true);
        }
    }
}