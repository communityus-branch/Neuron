using Static_Interface.API.EventFramework;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerQuitEvent : Event
    {
        public Player Player { get; }
        public string QuitMessage { get; set; }
        public PlayerQuitEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}