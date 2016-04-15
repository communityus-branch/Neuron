using System.Collections.Generic;
using Fclp.Internals.Extensions;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class Chat : NetworkedBehaviour
    {
        public List<string> ChatHistory = new List<string>();
        private Vector2 _scrollView = Vector2.zero;
        private string Message { get; set; } = "";
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
            Channel.Send(nameof(SendUserMessage), ECall.Server, EPacket.UPDATE_UNRELIABLE_BUFFER, text);
        }

        public void SendServerMessage(string text)
        {
            CheckServer();
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_UNRELIABLE_BUFFER, Channel.Connection.ServerID, text);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        private bool _justFocused;

        protected override void OnGUI()
        {
            base.OnGUI();
            if (InputUtil.Instance.IsInputLocked(this)) return;
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
                InputUtil.Instance.LockInput(this);
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
                InputUtil.Instance.UnlockInput(this);
            }
            if (_justFocused) _justFocused = false;
            if (string.IsNullOrEmpty(Message)) return;
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
            Channel.Send(nameof(ClearChatCommand), ECall.All, EPacket.UPDATE_UNRELIABLE_BUFFER);
        }

        [NetworkCall]
        private void SendUserMessage(Identity sender, string msg)
        {
            LogUtils.Debug(nameof(SendUserMessage));
            //Todo: onchatevent
            var userName = sender.GetUser()?.Name ?? "Server"; // if getuser returns null it means we are a server
            msg = "<color=yellow>" + userName + "</color>: " + msg;
            Channel.Send(nameof(ReceiveMessage), ECall.All, EPacket.UPDATE_UNRELIABLE_BUFFER, sender, msg);
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
        private void ClearChatCommand(Identity server)
        {
            if (!Channel.CheckServer(server)) return;
            ChatHistory.Clear();
        }
    }
}
