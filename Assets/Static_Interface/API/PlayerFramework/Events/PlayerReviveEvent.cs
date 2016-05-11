using Static_Interface.API.EventFramework;

namespace Static_Interface.API.PlayerFramework.Events
{
    public class PlayerReviveEvent : Event
    {
        public Player Player { get; private set; }

        public PlayerReviveEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}