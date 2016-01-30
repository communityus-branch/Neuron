﻿using Static_Interface.API.Event;
using Static_Interface.API.Player;

namespace Static_Interface.The_Collapse.Events
{
    public class PlayerLeftWaterEvent : Event
    {
        public Player Player { get; private set; }
        public PlayerLeftWaterEvent(Player player) : base(false)
        {
            Player = player;
        }
    }
}