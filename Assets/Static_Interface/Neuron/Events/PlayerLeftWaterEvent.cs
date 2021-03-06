﻿using Static_Interface.API.EventFramework;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.Neuron.Events
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