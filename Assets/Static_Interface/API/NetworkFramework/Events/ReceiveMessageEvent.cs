using Static_Interface.API.EventFramework;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.API.NetworkFramework.Events
{
    public class ReceiveMessageEvent : Event, ICancellable
    {
        public Identity Sender { get; }
        public string Message { get; set; }
        public bool IsPlayerMessage { get; }
        public ReceiveMessageEvent(Identity sender) : base(false)
        {
            IsPlayerMessage = sender?.GetUser()?.Player != null;
            Sender = sender;
        }

        public bool IsCancelled { get; set; }
    }
}