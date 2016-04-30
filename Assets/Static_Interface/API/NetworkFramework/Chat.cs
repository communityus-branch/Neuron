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
            Channel.Send(nameof(Network_SendUserMessage), ECall.Server, text);
        }

        public void SendServerMessage(string text)
        {
            CheckServer();
            Channel.Send(nameof(Network_ReceiveMessage), ECall.All, Channel.Connection.ServerID, text);
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
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
            if (IsServer())
            {
                Channel.Send(nameof(Network_ClearChatCommand), ECall.Clients);
            }
            ChatHistory.Clear();
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.SERVER)]
        private void Network_SendUserMessage(Identity sender, string msg)
        {
            LogUtils.Debug(nameof(Network_SendUserMessage));
            //Todo: onchatevent
            var userName = sender.GetUser()?.Name ?? "Server"; // if getuser returns null it means we are a server
            msg = "<color=yellow>" + userName + "</color>: " + msg;
            Channel.Send(nameof(Network_ReceiveMessage), ECall.All, sender, msg);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.BOTH, ValidateServer = true)]
        private void Network_ReceiveMessage(Identity server, Identity sender, string formattedMessage)
        {
            //Todo: onchatreceivedevent/onmessagereceived
            LogUtils.Debug(nameof(Network_ReceiveMessage));
            LogUtils.Log(formattedMessage);
            ChatHistory.Add(formattedMessage);
        }

        [NetworkCall(ConnectionEnd = ConnectionEnd.CLIENT, ValidateServer = true)]
        private void Network_ClearChatCommand(Identity server)
        {
            ClearChat();
        }
    }
}
