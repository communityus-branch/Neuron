﻿using System;
using Plugins.ConsoleUI.FrontEnd.UnityGUI;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.Internal.MultiplayerFramework;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.API.Utils
{
    public class InputUtil : PersistentScript<InputUtil>
    {
        private static bool InputLocked { get; set; } 
        private static object LockObject { get; set; }
        private static bool valBefore;
        public void LockInput(object o)
        {
            if(o == null) throw new ArgumentNullException(nameof(o));
            LogUtils.Debug(nameof(LockInput) + ": " + o.GetType().FullName);
            if (IsInputLocked(o)) throw new Exception("Input already locked by " + LockObject.GetType().FullName);
            InputLocked = true;
            LockObject = o;
            if(!(o is ConsoleGUI))
                ConsoleGUI.Instance.InputLocked = true;
            valBefore = GetMouseLookEnabled();
            SetMouseLookEnabled(false);
        }

        private bool GetMouseLookEnabled()
        {
            var mouseLook = Player.MainPlayer?.GetComponent<MouseLook>();
            return mouseLook == null || mouseLook.enabled;
        }

        public void SetMouseLookEnabled(bool v)
        {
            var mouseLook = Player.MainPlayer?.GetComponent<MouseLook>();
            if (mouseLook != null)
            {
                mouseLook.enabled = v;
            }
        }

        public void UnlockInput(object o)
        {
            LogUtils.Debug(nameof(UnlockInput) + ": " + o.GetType().FullName);
            if(o != LockObject) throw new ArgumentException("Input was not locked by given object!");
            ConsoleGUI.Instance.InputLocked = false;
            LockObject = null;
            InputLocked = false;
            SetMouseLookEnabled(valBefore);
        }

        public bool IsInputLocked(object o = null)
        {
            if (Connection.CurrentConnection != null && Connection.IsServer()) return false;
            if (o != null && o == LockObject) return false;
            return InputLocked;
        }

        private void CheckConsole()
        {
            if (!InputLocked && ConsoleGUI.Instance.IsOpen)
            {
                LockInput(ConsoleGUI.Instance);
            }
            else if (InputLocked && LockObject is ConsoleGUI && !ConsoleGUI.Instance.IsOpen)
            {
                UnlockInput(ConsoleGUI.Instance);
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            CheckConsole();
        }
    }
}