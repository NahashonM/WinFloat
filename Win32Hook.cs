using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WinFloat
{
    public static class Win32Hook
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

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_ESCAPE = 0x1B;


        private static IntPtr hookID = IntPtr.Zero;

        public static Action OnEscapeKeyPressed { get; set; }
        public static Action OnShortCutPressed { get; set; }


        public static bool RegisterWin32Hook()
        {
            if (hookID == IntPtr.Zero)
            {
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    LowLevelKeyboardProc proc = HookCallback;
                    hookID = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            return hookID != IntPtr.Zero;
        }


        public static bool UnRegisterWin32Hook()
        {
            if (hookID == IntPtr.Zero)
            {
                if(UnhookWindowsHookEx(hookID))
                    hookID = IntPtr.Zero;
            }

            return hookID == IntPtr.Zero;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (Win32Cursor.isCrossHairActive && nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == VK_ESCAPE)
                {
                    Debug.WriteLine("Escape Key pressed");
                    OnEscapeKeyPressed?.Invoke();
                }
            }
            
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

    }
}
