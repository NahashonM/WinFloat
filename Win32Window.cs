using System;
using System.Runtime.InteropServices;


namespace WinFloat
{
    internal class Win32Window
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private enum GetAncestorFlags
        {
            GA_PARENT = 1,      // Get the parent window.
            GA_ROOT = 2,        // Get the root window by walking the chain of parent windows.
            GA_ROOTOWNER = 3    // Get the owning window, or the top-level parent window if the window is unowned.
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags gaFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        // Window handles
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        // SetWindowPos flags
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;



        /// <summary>
        /// Get window at the cursor posision
        /// </summary>
        /// <returns>IntPtr: handle of the window at the cursor position</returns>
        public static IntPtr GetWindowUnderCursor()
        {
            POINT cursorPoint;

            if (!GetCursorPos(out cursorPoint))
                return IntPtr.Zero;

            IntPtr hWnd = WindowFromPoint(cursorPoint);
            if (hWnd == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr hWndContainingWindow = GetAncestor(hWnd, GetAncestorFlags.GA_ROOT);
            return hWndContainingWindow;
        }

        /// <summary>
        /// Get Foreground window
        /// </summary>
        /// <returns>IntPtr: handle of the foreground window</returns>
        public static IntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }

        /// <summary>
        /// Make a window the top most window.
        /// </summary>
        /// <param name="hwnd">IntPtr: Handle of the target window</param>
        /// <returns></returns>
        public static bool MakeWindowTopMost(IntPtr hwnd)
        {
            return SetWindowPos(hwnd, HWND_TOPMOST, 0,0, 0,0, SWP_NOSIZE | SWP_NOMOVE);
        }


        public static bool MakeWindowNonTopMost(IntPtr hwnd)
        {
            return SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }

    }
}
