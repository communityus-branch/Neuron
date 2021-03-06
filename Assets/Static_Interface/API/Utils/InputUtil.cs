﻿using System;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.WeaponFramework;
using Static_Interface.Internal;
using Static_Interface.Internal.MultiplayerFramework;
using ConsoleGUI = Static_Interface.API.ConsoleFramework.ConsoleGUI;

namespace Static_Interface.API.Utils
{
    public class InputUtil : PersistentScript<InputUtil>
    {
        private static bool InputLocked { get; set; } 
        private static object LockObject { get; set; }
        private static bool _valBefore;
        private static bool _wasCrosshairEnabled = true;
        public void LockInput(object o)
        {
            if(_pauselocked) throw new Exception("Game paused, can not lock input");
            if(o == null) throw new ArgumentNullException(nameof(o));
            LogUtils.Debug(nameof(LockInput) + ": " + o.GetType().FullName);
            if (IsInputLocked(o)) throw new Exception("Input already locked by " + LockObject.GetType().FullName);
            InputLocked = true;
            LockObject = o;
            _valBefore = GetMouseLookEnabled();
            SetMouseLookEnabled(false);
        }

        private bool GetMouseLookEnabled()
        {
            var mouseLook = Player.MainPlayer.MovementController.GetComponent<PlayerMouseLook>();
            return mouseLook != null && mouseLook.enabled;
        }

        public void SetMouseLookEnabled(bool v)
        {
            WeaponController weaponController = Player.MainPlayer?.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                if (v)
                {
                    weaponController.DrawCrosshair = _wasCrosshairEnabled;
                }
                else
                {
                    _wasCrosshairEnabled = weaponController.DrawCrosshair;
                    weaponController.DrawCrosshair = false;
                }
            }

            PlayerMouseLook playerMouseLook = Player.MainPlayer?.MovementController?.GetComponent<PlayerMouseLook>();
            if (playerMouseLook == null) return;
            playerMouseLook.enabled = v;
        }

        public bool IsGamePaused()
        {
            return !NetworkUtils.IsDedicated() && PauseHook.IsPaused;
        }


        public void UnlockInput(object o)
        {
            LogUtils.Debug(nameof(UnlockInput) + ": " + o.GetType().FullName);
            if(o != LockObject) throw new ArgumentException("Input was not locked by given object!");
            LockObject = null;
            InputLocked = false;
            SetMouseLookEnabled(_valBefore);
        }

        public bool IsInputLocked(object o = null)
        {
            if (Connection.IsDedicated) return false;
            if (o != null && o == LockObject) return false;
            if (_pauselocked) return true;
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

        bool _pauselocked;
        internal void PauseLockInput()
        {
            _pauselocked = true;
        }

        internal void PauseUnlockInput()
        {
            _pauselocked = false;
        }
    }
}