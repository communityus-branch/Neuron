using System;
using System.Collections.Generic;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class Chat : NetworkedBehaviour
    {
        public List<string> ChatHistory = new List<string>();
        private Vector2 _scrollView = Vector2.zero;
        private string _message = "";

        public bool Draw { get; set; } = true;

        public static Chat Instance;

        protected override void Start()
        {
            base.Start();
            Instance = this;
        }

        public void SendPlayerMessage(string text)
        {
            Channel.Send(nameof(SendUserMessage), ECall.Server, EPacket.UPDATE_RELIABLE_BUFFER, text);
        }

        public void SendServerMessage(string text)
        {
            if (!Connection.IsServer())
            {
                throw new Exception("This can be only called from server-side!");
            }
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_RELIABLE_BUFFER, Channel.Connection.ServerID, text);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        private void OnGUI()
        {
            if (!Draw) return;
            GUILayout.BeginArea(new Rect(0, 0, 400, 200));
            _scrollView = GUILayout.BeginScrollView(_scrollView);
            foreach (string c in ChatHistory)
            {
                GUILayout.Label(c);
            }
            GUILayout.EndArea();
            GUILayout.EndScrollView();
            _scrollView.y++;
            _message = GUI.TextField(new Rect(0, 200, 150, 25), _message);
            if (GUI.Button(new Rect(150, 200, 50, 25), "Send"))
            {
                SendPlayerMessage(_message);
                _message = "";
            }
        }

        [NetworkCall]
        public void SendUserMessage(Identity sender, string msg)
        {
            //Todo: onchatevent
            msg = "<color=yellow>" + sender.GetUser().Name + "</color>: " + msg;
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_RELIABLE_BUFFER, sender, msg);
        }

        [NetworkCall]
        public void ReceiveMessage(Identity server, Identity sender, string formattedMessage)
        {
            //Todo: onchatreceivedevent/onmessagereceived
            if (!Channel.CheckServer(server)) return;
            LogUtils.Log(formattedMessage);
            ChatHistory.Add(formattedMessage);
        }

        [NetworkCall]
        public void ClearChat(Identity server)
        {
            if (!Channel.CheckServer(server)) return;
            ChatHistory.Clear();
        }
    }
}
