using System;
using System.Collections.Generic;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PlayerInput : PlayerBehaviour
    {
        public const uint PERIOD = 250;
        private uint _lastSent;
        private List<KeyState> _keyStates = new List<KeyState>();
        private Vector3 _lookDirection = Vector3.zero;

        protected override void FixedUpdate()
        {
            if (InputUtil.IsInputLocked(this) || !Channel.IsOwner) return;
            _keyStates = new List<KeyState>();

            foreach (KeyCode keyCode in Enum.GetValues(typeof (KeyCode)))
            {
                KeyState state = new KeyState();
                if (Input.GetKey(keyCode))
                {
                    state.IsPressed = true;
                }

                if (Input.GetKeyDown(keyCode))
                {
                    state.IsDown = true;
                }

                state.KeyCode = (int) keyCode;

                if (state.IsPressed || state.IsDown) //only send pressed keys
                {
                    _keyStates.Add(state);
                }
            }

            bool send = TimeUtil.GetCurrentTime() - _lastSent > PERIOD;
            if (_keyStates.Count > 0 && send)
            {
                //Todo: OnKeyPressedEvent
                LogUtils.Debug("Sending " + _keyStates.Count + " key states");
                Channel.OpenWrite();
                Channel.Write(_keyStates.Count);
                foreach (KeyState state in _keyStates)
                {
                    Channel.Write(state);
                }
                Channel.CloseWrite(nameof(ReadInput), ECall.Server, EPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT);
                _lastSent = TimeUtil.GetCurrentTime();
            }

            if (Player.Camera.transform.eulerAngles != _lookDirection && send)
            {
                Channel.OpenWrite();
                Channel.Write(Player.Camera.transform.eulerAngles);
                Channel.CloseWrite(nameof(ReadLook), ECall.Server, EPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT);
            }
            _lookDirection = Player.Camera.transform.eulerAngles;

            Player.MovementController.HandleInput(this);
        }
        
        //ServerSide
        [NetworkCall]
        private void ReadInput(Identity id)
        {
            if (!Channel.CheckOwner(id)) return;
            _keyStates = new List<KeyState>();
            int size = Channel.Read<int>();
            for (int i = 0; i < size; i++)
            {
                KeyState state = Channel.Read<KeyState>();
                _keyStates.Add(state);
                if (state.IsDown)
                {
                    LogUtils.Log(Player.User.Name + " pressed " + state);
                }
            }

            //Todo: OnKeyPressedEvent
            Player.MovementController.HandleInput(this);
        }

        [NetworkCall]
        private void ReadLook(Identity id)
        {
            if (!Channel.CheckOwner(id)) return;
            _lookDirection = Channel.Read<Vector3>();
            if (Player.Health.IsDead) return;
            Vector3 newRot = Player.transform.eulerAngles;
            newRot.z= _lookDirection.z;
            Player.transform.eulerAngles = newRot;
        }

        public bool GetKey(KeyCode key)
        {
            KeyState state;
            try
            {
                state = _keyStates[(int) key];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            return state.IsPressed;
        }

        public bool GetKeyDown(KeyCode key)
        {
            KeyState state;
            try
            {
                state = _keyStates[(int)key];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            return state.IsDown;
        }

        public bool GetKeyUp(KeyCode key)
        {
            return !GetKeyDown(key);
        }

        public Vector3 GetLook()
        {
            return _lookDirection;
        }
    }
}