using Static_Interface.API.EventFramework;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.The_Collapse.Events
{
    public class PlayerEnteredWaterEvent : Event
    {
        public Player Player { get; private set; }
        public PlayerEnteredWaterEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}