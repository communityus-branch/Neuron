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
        internal List<KeyState> KeyStates = new List<KeyState>();
        private Vector3 _lookDirection = Vector3.zero;

        protected override void FixedUpdate()
        {
            //if (IsServer()) return; //Todo: dedicated server check
            if(!Channel.IsOwner) return;
            if (InputUtil.Instance.IsInputLocked(this) || !Channel.IsOwner) return;
            KeyStates = new List<KeyState>();

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

                if (state.IsPressed) //only process pressed keys
                {
                    KeyStates.Add(state);
                }
            }

            bool send = !(Connection.IsSinglePlayer && Channel.IsOwner) && !IsServer() && TimeUtil.GetCurrentTime() - _lastSent > PERIOD;
            Player.MovementController.UpdateInput(this);
            //Todo: OnKeyPressedEvent

            if (send)
            {
                if(KeyStates.Count > 0 ) LogUtils.Debug("Sending " + KeyStates.Count + " keystates on channel " + Channel.ID);
                Channel.Send(nameof(Network_ReadInput), ECall.Server, KeyStates.ToArray());
                _lastSent = TimeUtil.GetCurrentTime();

            }

            if (Player?.Camera?.transform == null || Player.MovementController == null) return;
            if (Player.Camera.transform.eulerAngles != _lookDirection && send)
            {
                Channel.Send(nameof(Network_ReadLook), ECall.Server, Player.Camera.transform.eulerAngles);
            }
            _lookDirection = Player.Camera.transform.eulerAngles;
        }
        
        //ServerSide
        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        private void Network_ReadInput(Identity id, KeyState[] states)
        {
            if(Player.MovementController == null) throw new Exception(id.Owner.Name + ": Player MovementController is null");
            KeyStates = states.ToList();
            //Todo: OnKeyPressedEvent
            Player.MovementController.UpdateInput(this);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER, ValidateOwner = true)]
        private void Network_ReadLook(Identity id, Vector3 dir)
        {
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
            if (!IsServer() || (IsServer() && Channel.IsOwner))
            {
                return Input.GetKey(key);
            }
            return (from state in KeyStates where state.KeyCode == (int) key select state.IsPressed).FirstOrDefault();
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
            if (!IsServer() || IsServer() && Channel.IsOwner)
            {
                return Input.GetKeyDown(key);
            }
            return (from state in KeyStates where state.KeyCode == (int)key select state.IsDown).FirstOrDefault();
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
            if (!IsServer() || IsServer() && Channel.IsOwner)
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