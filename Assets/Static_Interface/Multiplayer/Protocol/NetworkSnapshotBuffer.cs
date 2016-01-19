using UnityEngine;

namespace Static_Interface.Multiplayer.Protocol
{
    public class NetworkSnapshotBuffer
    {
        private Vector3 _lastPos;
        private Quaternion _lastRot;
        private readonly float _readDelay;
        private readonly float _readDuration;
        private int _readIndex;
        private float _readLast;
        private readonly NetworkSnapshot[] _snapshots = new NetworkSnapshot[0x10];
        private int _writeIndex;
        private float _writeLast;

        public NetworkSnapshotBuffer(float newDuration, float newDelay)
        {
            _readDuration = newDuration;
            _readDelay = newDelay;
        }

        public void AddNewSnapshot(Vector3 pos, Quaternion rot)
        {
            _snapshots[_writeIndex].pos = pos;
            _snapshots[_writeIndex].rot = rot;
            _snapshots[_writeIndex].timestamp = Time.realtimeSinceStartup;
            IncrementWriteIndex();
            _writeLast = Time.realtimeSinceStartup;
        }

        public void GetCurrentSnapshot(out Vector3 pos, out Quaternion rot)
        {
            int num = GetReadWriteSpace();
            if (num <= 0)
            {
                _readLast = Time.realtimeSinceStartup;
                pos = _lastPos;
                rot = _lastRot;
            }
            else if (num > (_snapshots.Length - 2))
            {
                _readIndex = _writeIndex;
                pos = _lastPos;
                rot = _lastRot;
            }
            else if ((Mathf.Max(_writeLast, Time.realtimeSinceStartup) - _snapshots[_readIndex].timestamp) < _readDelay)
            {
                _readLast = Time.realtimeSinceStartup;
                pos = _lastPos;
                rot = _lastRot;
            }
            else
            {
                if ((Time.realtimeSinceStartup - _readLast) > _readDuration)
                {
                    _lastPos = _snapshots[_readIndex].pos;
                    _lastRot = _snapshots[_readIndex].rot;
                    IncrementReadIndex();
                    _readLast = Time.realtimeSinceStartup;
                }
                float t = Mathf.Clamp01((Time.realtimeSinceStartup - _readLast) / _readDuration);
                pos = Vector3.Lerp(_lastPos, _snapshots[_readIndex].pos, t);
                rot = Quaternion.Slerp(_lastRot, _snapshots[_readIndex].rot, t);
            }
        }

        private int GetReadWriteSpace()
        {
            if (_readIndex <= _writeIndex)
            {
                return (_writeIndex - _readIndex);
            }
            return (_writeIndex + (_snapshots.Length - _readIndex));
        }

        private void IncrementReadIndex()
        {
            _readIndex++;
            if (_readIndex == _snapshots.Length)
            {
                _readIndex = 0;
            }
        }

        private void IncrementWriteIndex()
        {
            _writeIndex++;
            if (_writeIndex == _snapshots.Length)
            {
                _writeIndex = 0;
            }
        }

        public void UpdateLastSnapshot(Vector3 pos, Quaternion rot)
        {
            _readIndex = 0;
            _writeIndex = 0;
            _lastPos = pos;
            _lastRot = rot;
            _readLast = Time.realtimeSinceStartup;
        }
    }
}
