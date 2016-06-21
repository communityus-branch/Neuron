using System;
using System.IO;
using Static_Interface.API.CommandFramework;
using Static_Interface.API.EventFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SchedulerFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.VehicleFramework;

namespace Static_Interface.API.PluginFramework
{
    /// <summary>
    /// Base plugin class. Every mod/gamemode etc may only have one plugin class in the whole assembly.
    /// </summary>
    public abstract class Plugin
    {
        internal string Path;
        private bool _enabled;

        /// <summary>
        /// Human-Readable name of the plugin
        /// </summary>
        public readonly string Name;

        protected Plugin()
        {
            Name = GetType().Name;
            //Todo: assign Name from attribute
        }

        public string DataDir => Directory.GetParent(Path).FullName;

        /// <summary>
        /// Get or set the enabled status of the plugin
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                if (_enabled && !value)
                {
                    Disable();
                }
                else if (!_enabled && value)
                {
                    Enable();
                }
            }
        }

        private void Enable()
        {
            if (_enabled) return;
            _enabled = true;
            LogUtils.Log("Enabling plugin: " + Name + " (" + GetType().Name + ")");
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
        /// Called when the plugin gets enabled
        /// </summary>
        protected virtual void OnEnable() { }

        private void Disable()
        {
            if (!_enabled) return;
            LogUtils.Log("Disabling plugin: " + Name + " (" + GetType().Name + ")");
            Scheduler.Instance?.RemoveAllTasks(this);
            EventManager.Instance?.ClearListeners(this);
            CommandManager.Instance?.OnPluginDisabled(this);
            VehicleManager.Instance?.OnPluginDisabled(this);
            PlayerModel.OnPluginUnload(this);

            try
            {
                OnDisable();
            }
            catch (Exception e)
            {
                e.Log("Exception while disabling plugin: " + Name);
            }
            _enabled = false;
        }

        /// <summary>
        /// Called when the plugin get disabled
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