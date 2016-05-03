using System;
using Static_Interface.API.EventFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;

namespace Static_Interface.API.ExtensionFramework
{
    /// <summary>
    /// Base extension class. Every mod/gamemode etc may only have one extension class in the whole assembly.
    /// </summary>
    public abstract class Extension
    {
        internal string Path;
        private bool _enabled;

        /// <summary>
        /// Human-Readable name of the extension
        /// </summary>
        public readonly string Name;

        public Extension()
        {
            Name = GetType().Name;
            //Todo: assign Name from attribute
        }

        /// <summary>
        /// Get or set the enabled status of the extension
        /// </summary>
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
                e.Log("Exception while enabling plugin: " + Name);
            }
        }

        /// <summary>
        /// Called when the extension gets enabled
        /// </summary>
        protected virtual void OnEnable() { }

        private void Disable()
        {
            if (!_enabled) return;
            LogUtils.Log("Disabling extension: " + Name + " (" + GetType().Name + ")");
            Scheduler.Instance.RemoveAllTasks(this);
            EventManager.Instance.ClearListeners(this);
            try
            {
                OnDisable();
            }
            catch (Exception e)
            {
                e.Log("Exception while disabling plugin: " + Name);
            }
        }

        /// <summary>
        /// Called when the extension get disabled
        /// </summary>
        protected virtual void OnDisable() { }

        /// <summary>
        /// Called with Unity's Update Method
        /// </summary>
        public virtual void Update() { }
        
        /// <summary>
        /// Called with Unity's FixedUpdate Method
        /// </summary>
        public virtual void FixedUpdate() { }
    }
}