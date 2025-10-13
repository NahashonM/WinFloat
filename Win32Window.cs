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


        /// <summary>
        /// Get window at the cursor posision
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public static IntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }

    }
}
