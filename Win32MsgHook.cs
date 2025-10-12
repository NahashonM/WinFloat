using System;
using Windows.System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WinFloat
{
    public static class Win32MsgHook
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        


        private static IntPtr hookID = IntPtr.Zero;

        private static  Dictionary<VirtualKey, Dictionary<int, KBShortcut>> shortcuts = 
            new Dictionary<VirtualKey, Dictionary<int, KBShortcut>>();


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;


        /// <summary>
        /// Register Win32 Message Hook
        /// </summary>
        /// <returns></returns>
        public static bool RegisterWin32Hook()
        {
            if (hookID == IntPtr.Zero)
            {
                using (var curProcess = Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    LowLevelKeyboardProc proc = HookCallback;
                    hookID = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            return hookID != IntPtr.Zero;
        }

        /// <summary>
        /// Remove SetWindowsHookEx hook procedure
        /// </summary>
        /// <returns></returns>
        public static bool UnRegisterWin32Hook()
        {
            if (hookID == IntPtr.Zero)
            {
                if(UnhookWindowsHookEx(hookID))
                    hookID = IntPtr.Zero;
            }

            return hookID == IntPtr.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shortcut"></param>
        public static void AddShortcut(KBShortcut shortcut)
        {
            if (!shortcuts.ContainsKey(shortcut.MainKey))
            {
                shortcuts.Add(shortcut.MainKey, new Dictionary<int, KBShortcut>());
            }

            if (shortcuts.TryGetValue(shortcut.MainKey, out Dictionary<int, KBShortcut> keyDict))
            {
                keyDict.Add(shortcut.GetHashCode(), shortcut);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        public static bool RemoveShortcut(KBShortcut shortcut)
        {
            if (shortcuts.TryGetValue(shortcut.MainKey, out Dictionary<int, KBShortcut> keyDict))
            {
                bool removed = keyDict.Remove(shortcut.GetHashCode());

                if (keyDict.Count == 0)
                {
                    shortcuts.Remove(shortcut.MainKey);
                }

                return removed;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(nCode >= 0 && (int)wParam == WM_KEYDOWN)
            {
                VirtualKey vkCode = (VirtualKey)Marshal.ReadInt32(lParam);

                if (shortcuts.TryGetValue(vkCode, out Dictionary<int, KBShortcut> keyDict))
                {
                    bool ctrlState = (GetKeyState((int)VirtualKey.Control) & 0x8000) != 0;
                    bool altState = (GetKeyState((int)VirtualKey.Menu) & 0x8000) != 0;
                    bool shiftState = (GetKeyState((int)VirtualKey.Shift) & 0x8000) != 0;
                    bool winLState = (GetKeyState((int)VirtualKey.LeftWindows) & 0x8000) != 0;
                    bool winRState = (GetKeyState((int)VirtualKey.RightWindows) & 0x8000) != 0;

                    int hashCode = KBShortcut.GenHashCode(winRState|| winLState, ctrlState, shiftState, altState, vkCode);
                    
                    if (keyDict.TryGetValue(hashCode, out KBShortcut shortcut))
                    {
                        shortcut.OnShortCutActive?.Invoke();
                    }
                }
            }
            
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

    }
}
