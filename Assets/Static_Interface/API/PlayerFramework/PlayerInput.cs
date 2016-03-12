using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
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
                Channel.Send(nameof(ReadInput), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, new object[] {_keyStates.ToArray()});
                _lastSent = TimeUtil.GetCurrentTime();
            }

            if (Player?.Camera?.transform == null || Player.MovementController == null) return;
            if (Player.Camera.transform.eulerAngles != _lookDirection && send)
            {
                Channel.Send(nameof(ReadLook), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, Player.Camera.transform.eulerAngles);
            }
            _lookDirection = Player.Camera.transform.eulerAngles;

            if (Connection.IsSinglePlayer) return;
            Player.MovementController.HandleInput(this);
        }
        
        //ServerSide
        [NetworkCall]
        private void ReadInput(Identity id, KeyState[] states)
        {
            if (!Channel.CheckOwner(id)) return;
            _keyStates = states.ToList();
            LogUtils.Debug("Received " + _keyStates.Count + " key states");
            //Todo: OnKeyPressedEvent
            Player.MovementController.HandleInput(this);
        }

        [NetworkCall]
        private void ReadLook(Identity id, Vector3 dir)
        {
            if (!Channel.CheckOwner(id)) return;
            _lookDirection = dir;
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