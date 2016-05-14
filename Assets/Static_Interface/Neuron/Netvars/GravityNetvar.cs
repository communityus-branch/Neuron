using Static_Interface.API.NetvarFramework;
using UnityEngine;

namespace Static_Interface.Neuron.Netvars
{
    public class GravityNetvar : Netvar
    {
        private Vector3 _defaultValue;

        protected override void OnPreInit()
        {
            base.OnPreInit();
            _defaultValue = Physics.gravity;
        }

        public override string Name => "sv_gravity";

        protected override void OnSetValue(object oldValue, object newValue)
        {
            Physics.gravity = (Vector3) newValue;
        }

        public override object GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}