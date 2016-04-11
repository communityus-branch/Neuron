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
            //if (Connection.IsServer()) return; //Todo: dedicated server check
            if(!Channel.IsOwner) return;
            if (InputUtil.Instance.IsInputLocked(this) || !Channel.IsOwner) return;
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

            bool send = !Connection.IsSinglePlayer && !Connection.IsServer() && TimeUtil.GetCurrentTime() - _lastSent > PERIOD;
            Player.MovementController.UpdateInput(this);
            //Todo: OnKeyPressedEvent
            //LogUtils.Debug("Sending " + _keyStates.Count + " key states");
            if (send)
            {
                Channel.Send(nameof(ReadInput), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, _keyStates.ToArray());
                _lastSent = TimeUtil.GetCurrentTime();
            }

            if (Player?.Camera?.transform == null || Player.MovementController == null) return;
            if (Player.Camera.transform.eulerAngles != _lookDirection && send)
            {
                Channel.Send(nameof(ReadLook), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, Player.Camera.transform.eulerAngles);
            }
            _lookDirection = Player.Camera.transform.eulerAngles;
        }
        
        //ServerSide
        [NetworkCall]
        private void ReadInput(Identity id, KeyState[] states)
        {
            if(states == null) throw new ArgumentNullException(nameof(states));
            if(Channel == null) throw new Exception(id.Owner?.Name + ": Channel is null");
            if(Player == null) throw new Exception(id.Owner?.Name + ": Player is null");
            if(Player.MovementController == null) throw new Exception(id.Owner?.Name + ": Player MovementController is null");
            if (!Channel.CheckOwner(id)) return;
            _keyStates = states.ToList();
            LogUtils.Debug("Received " + _keyStates.Count + " key states from " + id.Owner?.Name);
            //Todo: OnKeyPressedEvent
            Player.MovementController.UpdateInput(this);
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

        /// <summary>
        /// 
        /// <para>
        /// Returns true while the player holds down the key identified by the key KeyCode enum parameter.
        /// </para>
        /// 
        /// </summary>
        /// <param name="key"/>
        public bool GetKey(KeyCode key)
        {
            if (!Connection.IsServer() || Connection.IsSinglePlayer)
            {
                return Input.GetKey(key);
            }
            return (from state in _keyStates where state.KeyCode == (int) key select state.IsPressed).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// <para>
        /// Returns true during the frame the player starts pressing down the key identified by the key KeyCode enum parameter.
        /// </para>
        /// 
        /// </summary>
        /// <param name="key"/>
        public bool GetKeyDown(KeyCode key)
        {
            if (!Connection.IsServer() || Connection.IsSinglePlayer)
            {
                return Input.GetKeyDown(key);
            }
            return (from state in _keyStates where state.KeyCode == (int)key select state.IsDown).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// <para>
        /// Returns true during the frame the player releases the key identified by the key KeyCode enum parameter.
        /// </para>
        /// 
        /// </summary>
        /// <param name="key"/>
        public bool GetKeyUp(KeyCode key)
        {
            if (!Connection.IsServer() || Connection.IsSinglePlayer)
            {
                return Input.GetKeyUp(key);
            }
            return !GetKeyDown(key);
        }
        /// <summary>
        /// 
        /// <para>
        /// Returns the direction the player is facing at 
        /// </para>
        /// 
        /// </summary>

        public Vector3 GetLook()
        {
            return _lookDirection;
        }
    }
}