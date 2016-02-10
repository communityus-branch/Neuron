using System;
using Plugins.ConsoleUI.FrontEnd.UnityGUI;

namespace Static_Interface.API.Utils
{
    public class GuiUtil
    {
        private static bool InputLocked { get; set; } 
        private static object LockObject { get; set; }

        public static void LockInput(object o)
        {
            if(o == null) throw new ArgumentNullException(nameof(o));
            LogUtils.Debug(nameof(LockInput) + ": " + o.GetType().FullName);
            if (IsInputLocked(o)) throw new Exception("Input already locked by " + LockObject.GetType().FullName);
            InputLocked = true;
            LockObject = o;
            ConsoleGUI.Instance.LockInput = true;
        }

        public static void UnlockInput(object o)
        {
            LogUtils.Debug(nameof(UnlockInput) + ": " + o.GetType().FullName);
            if(o != LockObject) throw new ArgumentException("Input was not locked by given object!");
            ConsoleGUI.Instance.LockInput = false;
            LockObject = null;
            InputLocked = false;
        }

        public static bool IsInputLocked(object o = null)
        {
            if (ConsoleGUI.Instance.IsOpen)
            {
                return true;
            }
            if (o != null && o == LockObject) return false;
            return InputLocked;
        }
    }
}