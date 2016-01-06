using Assets.API.EventFramework;

namespace Assets.Events
{
    public class PlayerEnteredWaterEvent : Event
    {
        public PlayerEnteredWaterEvent() : base(false)
        {

        }
    }
}