using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyPositionSyncer : NetworkedBehaviour
    {
        private Rigidbody Rigidbody => GetComponent<Rigidbody>();
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
            _syncTime += Time.deltaTime;
            if (_syncStartPosition == null || _syncEndPosition == null) return;
            var vec = Vector3.Lerp(_syncStartPosition.Value, _syncEndPosition.Value, _syncTime / _syncDelay);
            Rigidbody.position = vec;

            if (vec == _syncEndPosition.Value)
            {
                _syncStartPosition = null;
                _syncEndPosition = null;
            }
        }

        protected override bool OnSync()
        {
            base.OnSync();
            if (_cachedPosition == Rigidbody.position && _cachedVelocity == Rigidbody.velocity)
            {
                // no changes, no need for updates
                return false;
            }

            _cachedPosition = Rigidbody.position;
            _cachedVelocity = Rigidbody.velocity;
            Channel.Send(nameof(Network_ReadPositionServer), ECall.Server, (object)Rigidbody.position, Rigidbody.velocity);
            return true;
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        protected void Network_ReadPositionServer(Identity ident, Vector3 syncPosition, Vector3 syncVelocity)
        {
            if (ident != Connection.ServerID)
            {
                if (_positionValidators.Count > 0)
                {
                    var deltaPosition = syncPosition - Rigidbody.position;
                    var deltaVelocity = syncVelocity - Rigidbody.velocity;
                    if (_positionValidators.Any(val => !val.ValidatePosition(ident, Rigidbody.transform, deltaPosition, deltaVelocity)))
                    {
                        Channel.Send(nameof(Network_ReadPositionClient), ECall.Owner, (object) Rigidbody.position,
                            Rigidbody.velocity, true);
                        return;
                    }
                }

                ReadPosition(syncPosition, syncVelocity, IsDedicatedServer());
            }

            try
            {
                var body = Rigidbody;
                if(!body)
                    throw new MissingReferenceException("Rigidbody not found");
            }
            catch (Exception)
            {
                Destroy(this);
                return;
            }
            Channel.Send(nameof(Network_ReadPositionClient), ECall.NotOwner, Rigidbody.position, syncPosition, syncVelocity, false);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer= true, MaxRadius = 1000)]
        protected void Network_ReadPositionClient(Identity ident, Vector3 syncPosition, Vector3 syncVelocity, bool force)
        {
            ReadPosition(syncPosition, syncVelocity, force);
        }

        private void ReadPosition(Vector3 syncPosition, Vector3 syncVelocity, bool snap)
        {
            if (snap)
            {
                Rigidbody.position = syncPosition;
                Rigidbody.velocity = syncVelocity;
                return;
            }
            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
            LastSync = TimeUtil.GetCurrentTime();
            _syncEndPosition = syncPosition + syncVelocity * _syncDelay;
            _syncStartPosition = Rigidbody.position;
        }

        public static void AddRigidbodySyncer(GameObject obj, Channel ch)
        {
            obj.SetActive(false);
            var syncer = obj.AddComponent<RigidbodyPositionSyncer>();
            syncer.Channel = ch;
            obj.SetActive(true);
        }
    }
}