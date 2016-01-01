using System;
using Assets.API.Netvar;
using UnityEngine;

namespace Assets.Netvars
{
    public class GravityNetvar : Netvar
    {
        private readonly float _defaultValue;
        public GravityNetvar()
        {
            _defaultValue = Physics.gravity.y;
        }
        public override string Name
        {
            get { return "sv_gravity"; }
        }

        protected override object OnGetValue()
        {
            return Physics.gravity.y;
        }

        protected override void OnSetValue(object oldValue, object newValue)
        {
            Physics.gravity = new Vector3(0, (float) newValue, 0);
        }

        public override object GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}