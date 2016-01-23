using System;
using Static_Interface.Internal;

namespace Static_Interface.API.ExtensionsFramework
{
    public abstract class Extension
    {
        internal string Path;
        private bool _enabled;


        public readonly string Name;

        public Extension()
        {
            //Todo: assign Name from attribute
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled && !value) Disable();
                else if (!_enabled && value) Enable();
                _enabled = value;
            }
        }

        private void Enable()
        {
            if (_enabled) return;
            LogUtils.Log("Enablign extension: " + Name + " (" + GetType().Name + ")");
            try
            {
                OnEnable();
            }
            catch (Exception e)
            {
                e.Log("Couldn't enable plugin: " + Name);
                Disable();
            }
        }

        protected virtual void OnEnable() {}

        private void Disable()
        {
            if (!_enabled) return;
            LogUtils.Log("Disabling extension: " + Name + " (" + GetType().Name + ")");
            try
            {
                OnDisable();
            }
            catch (Exception e)
            {
                e.Log("Exception while disabling plugin: " + Name);
            }
        }

        protected virtual void OnDisable() { }

        public virtual void Update() { }
    }
}