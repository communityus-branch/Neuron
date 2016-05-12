using System;
using System.Security.Policy;
using Static_Interface.API.EventFramework;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.API.NetworkFramework.Events
{
    public class PlayerChatEvent : Event, ICancellable
    {
        public Player Player { get; }
        public string Format { get; set; }
        public string Message { get; set; }
        public PlayerChatEvent(Player player) : base(false)
        {
            Player = player;
        }

        public bool IsCancelled { get; set; }
    }
}