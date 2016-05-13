using Static_Interface.API.NetvarFramework;
using UnityEngine;

namespace Static_Interface.Neuron.Netvars
{
    public class GameSpeedNetvar : Netvar
    {
        public override string Name => "sv_speed";

        protected override void OnSetValue(object oldValue, object newValue)
        {
            base.OnSetValue(oldValue, newValue);
            Time.timeScale = (float)newValue;
        }

        public override object GetDefaultValue()
        {
            return 1f;
        }
    }
}