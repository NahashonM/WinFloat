using System;
using Windows.System;
using System.Collections.Generic;


namespace WinFloat
{
    public class KBShortcut
    {
        private bool WinKey { get; set; }
        private bool CtrlKey { get; set; }
        private bool ShiftKey { get; set; }
        private bool AltKey { get; set; }

        public VirtualKey MainKey { get; set; }

        public Action OnShortCutActive { get; set; }

        //(Windows.System.VirtualKey) System.Enum.Parse(typeof(Windows.System.VirtualKey), mainKey.ToUpper());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="winkey"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="mainKey"></param>
        /// <param name="onActive"></param>
        public KBShortcut(bool winkey, bool control, bool shift, bool alt, VirtualKey mainKey, Action onActive)
        {
            WinKey = winkey;
            CtrlKey = control;
            ShiftKey = shift;
            AltKey = alt;
            MainKey = mainKey;
            OnShortCutActive = onActive;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> parts = new List<string>();

            if (WinKey) parts.Add("Win");
            if (CtrlKey) parts.Add("Ctrl");
            if (ShiftKey) parts.Add("Shift");
            if (AltKey) parts.Add("Alt");

            parts.Add(MainKey.ToString());

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GenHashCode(WinKey, CtrlKey, ShiftKey, AltKey, MainKey);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="winkey"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="mainKey"></param>
        /// <returns></returns>
        public static int GenHashCode(bool winkey, bool control, bool shift, bool alt, VirtualKey mainKey)
        {
            int hashCode = 0;

            if (winkey) hashCode = VirtualKey.RightWindows.GetHashCode() | VirtualKey.LeftWindows.GetHashCode();
            if (control) hashCode = hashCode | VirtualKey.Control.GetHashCode();
            if (shift) hashCode = hashCode | VirtualKey.Shift.GetHashCode();
            if (alt) hashCode = hashCode | VirtualKey.Menu.GetHashCode();
            
            hashCode = hashCode | mainKey.GetHashCode();

            return hashCode;
        }
    }
}
