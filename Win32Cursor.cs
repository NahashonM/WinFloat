
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WinFloat
{
    internal static class Win32Cursor
    {
        // ------------ Cursor Appearance ------------
        private const uint OCR_CROSS = 32515;
        private const uint OCR_NORMAL = 32512;
        private const uint SPI_SETCURSORS = 0x0057;
        private const uint SPIF_SENDCHANGE = 0x0002;

        public static bool isCrossHairActive = false;


        // ------------ Mouse Hook ------------
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONUP = 0x0202;

        private static IntPtr hookId = 0;
        private static HashSet<Action> MouseUpHandlers = new HashSet<Action>();

        private delegate IntPtr fn_HookProc(int nCode, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Initialize()
        {
            if (hookId != IntPtr.Zero) return true;

            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                hookId = SetWindowsHookEx(WH_MOUSE_LL, HookProc, GetModuleHandle(curModule.ModuleName), 0);
            }

            return hookId != IntPtr.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void Destroy()
        {
            MouseUpHandlers.Clear();

            if (hookId == IntPtr.Zero) return;

            if (UnhookWindowsHookEx(hookId))
                hookId = IntPtr.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onMouseUp"></param>
        /// <returns></returns>
        public static bool AddOnMouseUpHandler(Action onMouseUp)
        {
            return MouseUpHandlers.Add(onMouseUp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onMouseUp"></param>
        /// <returns></returns>
        public static bool RemoveOnMouseUpHandler(Action onMouseUp)
        {
            return MouseUpHandlers.Remove(onMouseUp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(nCode >= 0 && (int)wParam == WM_LBUTTONUP && MouseUpHandlers.Count > 0)
            {
                Task.Run(() => {
                    foreach (var handler in MouseUpHandlers)
                    {
                        handler.Invoke();
                    }
                });
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }



        public static void SetGlobalCrossCursor()
        {
            if (isCrossHairActive) return;

            try
            {
                // Load the cross cursor
                IntPtr crossCursor = LoadCursor(IntPtr.Zero, (int)OCR_CROSS);
                if (crossCursor == IntPtr.Zero)
                {
                    //Debug.WriteLine("Failed to load cross cursor");
                    return;
                }

                // Set system cursor to cross
                if (SetSystemCursor(crossCursor, OCR_NORMAL))
                {
                    isCrossHairActive = true;
                    return;
                }

                //Debug.WriteLine("Failed to set system cursor");
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error setting global cursor: {ex.Message}");
            }
        }


        public static void RestoreGlobalCursor()
        {
            if (!isCrossHairActive) return;

            try
            {
                // Reset all system cursors to defaults
                SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_SENDCHANGE);
                isCrossHairActive = false;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error restoring cursor: {ex.Message}");
            }
        }

        #region DLL Imports
        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, fn_HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
