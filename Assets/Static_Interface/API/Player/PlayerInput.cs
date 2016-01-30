using System;
using System.Collections.Generic;
using Static_Interface.API.Network;
using Static_Interface.API.Utils;
using Steamworks;
using UnityEngine;
using Types = Static_Interface.Internal.Objects.Types;

namespace Static_Interface.API.Player
{
    public class PlayerInput : PlayerBehaviour
    {
        private List<KeyState> _keyStates = new List<KeyState>(); 
        protected override void FixedUpdate()
        {
            if (Channel.IsOwner)
            {
                _keyStates = new List<KeyState>();

                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
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

                    state.KeyCode = (int)keyCode;

                    if (state.IsPressed || state.IsDown) //only send pressed keys
                    {
                        _keyStates.Add(state);
                    }
                }


                Channel.OpenWrite();
                Channel.Write(_keyStates.Count);
                foreach (KeyState state in _keyStates)
                {
                    Channel.Write(state);
                }
                Channel.CloseWrite(nameof(ReadInput), ECall.Server, EPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT);

            }

            if (_keyStates.Count == 0) return;
            Player.MovementController.HandleInput(this);
        }

        //ServerSide
        [NetworkCall]
        public void ReadInput(CSteamID id)
        {
            if (!Channel.CheckOwner(id)) return;
            _keyStates = new List<KeyState>();
            int size = (int) Channel.Read(Types.INT32_TYPE)[0];
            for (int i = 0; i < size; i++)
            {
                KeyState state = (KeyState) Channel.Read(Types.KEYSTATE_TYPE)[0];
                _keyStates.Add(state);
                if (state.IsDown)
                {
                    LogUtils.Log(Player.User.Identity.PlayerName + " pressed " + ((KeyCode)state.KeyCode));
                }
            }

            //Todo: OnKeyPressedEvent
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
    }
}