using Static_Interface.API.Netvar;
using UnityEngine;

namespace Static_Interface.The_Collapse.Netvars
{
    public class GravityNetvar : Netvar
    {
        private readonly float _defaultValue;
        public GravityNetvar()
        {
            _defaultValue = Physics.gravity.y;
        }

        public override string Name => "sv_gravity";

        protected override void OnSetValue(object oldValue, object newValue)
        {
            Physics.gravity = new Vector3(0, -(float) newValue, 0);
        }

        public override object GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}