using System;
using System.Collections.Generic;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Utils;
using Steamworks;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public class PlayerInput : PlayerBehaviour
    {
        void FixedUpdate()
        {
            if (!ConnectionUtils.IsServer())
            {
                List<KeyState> keyStates = new List<KeyState>();

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

                    if (state.IsPressed || state.IsDown)
                    {
                        keyStates.Add(state);
                    }
                }


            }
            else
            {
                Channel.OpenWrite();
            }
        }

        //ServerSide
        [RPCCall]
        public void GetInput(CSteamID id)
        {
            if (!Channel.CheckOwner(id)) return;

        }

        [RPCCall]
        public void AskAck(CSteamID id)
        {
            if (!Channel.ValidateServer(id)) return;

        }
    }
}