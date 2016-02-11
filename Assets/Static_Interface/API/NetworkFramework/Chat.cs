using System;
using System.Collections.Generic;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class Chat : NetworkedBehaviour
    {
        public List<string> ChatHistory = new List<string>();
        private Vector2 _scrollView = Vector2.zero;
        public string Message { get; set; } = "";
        private const string ChatTextFieldName = "ChatTextField";
        public bool Draw { get; set; } = true;
        public static Chat Instance;

        private bool ChatTextFieldFocused { get; set; }
        public bool ChatTextFieldVisible { get; set; }
        protected override void Start()
        {
            base.Start();
            Instance = this;
        }

        public void SendPlayerMessage(string text)
        {
            Channel.Send(nameof(SendUserMessage), ECall.Server, EPacket.UPDATE_RELIABLE_CHUNK_BUFFER, text);
        }

        public void SendServerMessage(string text)
        {
            CheckServer();
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_RELIABLE_CHUNK_BUFFER, Channel.Connection.ServerID, text);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        private bool _justFocused;

        private void OnGUI()
        {
            if (InputUtil.IsInputLocked(this)) return;
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

            if (!ChatTextFieldVisible)
            {
                if (!Input.GetKeyDown(KeyCode.Return)) return;
                InputUtil.LockInput(this);
                ChatTextFieldVisible = true;
                FoucsChatTextField();
                _justFocused = true;
                return;
            }

#region DrawTextField
            GUI.SetNextControlName(ChatTextFieldName);
            Message = GUI.TextField(new Rect(0, 200, 150, 25), Message);
            if (ChatTextFieldFocused)
            {
                GUI.FocusControl(ChatTextFieldName);
                ChatTextFieldFocused = false;
            }
#endregion

            bool returnPressed = (Event.current.type == EventType.keyDown  && Event.current.character == '\n');

            if (!IsChatTextFieldFocused() || !returnPressed) return;
            if (!_justFocused)
            {
                ChatTextFieldVisible = false;
                InputUtil.UnlockInput(this);
            }
            if (_justFocused) _justFocused = false;
            if (string.IsNullOrEmpty(Message?.Trim()))
            {
                return;
            }
            SendPlayerMessage(Message);
            Message = "";
        }
        
        public bool IsChatTextFieldFocused()
        {
            return GUI.GetNameOfFocusedControl() == ChatTextFieldName;
        }

        public void FoucsChatTextField()
        {
            ChatTextFieldFocused = true;
        }

        public void ClearChat()
        {
            CheckServer();
            Channel.Send(nameof(ClearChat), ECall.All, EPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
        }

        [NetworkCall]
        private void SendUserMessage(Identity sender, string msg)
        {
            //Todo: onchatevent
            var userName = sender.GetUser()?.Name ?? "Console";
            msg = "<color=yellow>" + userName + "</color>: " + msg;
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_RELIABLE_CHUNK_BUFFER, sender, msg);
        }

        [NetworkCall]
        private void ReceiveMessage(Identity server, Identity sender, string formattedMessage)
        {
            //Todo: onchatreceivedevent/onmessagereceived
            LogUtils.Debug(nameof(ReceiveMessage));
            if (!Channel.CheckServer(server)) return;
            LogUtils.Log(formattedMessage);
            ChatHistory.Add(formattedMessage);
        }

        [NetworkCall]
        private void ClearChat(Identity server)
        {
            if (!Channel.CheckServer(server)) return;
            ChatHistory.Clear();
        }
    }
}
