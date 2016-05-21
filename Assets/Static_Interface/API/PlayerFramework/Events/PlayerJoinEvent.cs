using Static_Interface.API.EventFramework;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerJoinEvent : Event
    {
        public Player Player { get; }
        public string JoinMessage { get; set; }
        public PlayerJoinEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}