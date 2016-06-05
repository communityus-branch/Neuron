using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeaponFramework;
using UnityEngine;

namespace Static_Interface.Internal
{
    public static class PauseHook
    {
        //We need to modify bl_PauseMenu.cs from UPause.
        //At DoPause(), below the if(m_Pause) insert a call to the OnPause() method here
        //at the else insert a call to OnResume()
        private static bool _wasCursorVisible;
        private static bool _wasCrosshairVisible;
        internal static void OnPause()
        {
            _wasCursorVisible = Cursor.visible;
            Cursor.visible = true;
            InputUtil.Instance.PauseLockInput();
            
            if (Player.MainPlayer.GetComponent<WeaponController>())
            {
                _wasCrosshairVisible = Player.MainPlayer.GetComponent<WeaponController>();
                Player.MainPlayer.GetComponent<WeaponController>().DrawCrosshair = false;
            }
        }

        internal static void OnResume()
        {
            Cursor.visible = _wasCursorVisible;
            InputUtil.Instance.PauseUnlockInput();
            Player.MainPlayer.GetComponent<WeaponController>().DrawCrosshair = _wasCrosshairVisible;
        }
    }
}