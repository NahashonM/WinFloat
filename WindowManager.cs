
using System.Text;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Application = System.Windows.Application;

namespace WinFloat
{
    internal class Win32Window:IDisposable
    {

        public struct WINDOWINFO
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
            public uint ProcessId { get; set; }
            public bool IsTopmost { get; set; }

            public override string ToString()
            {
                return $"HWND: 0x{Handle.ToInt64():X}, PID: {ProcessId}, Title: \"{Title}\"";
            }
        }

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


        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        // Window Position flags
        private const int SWP_NOSIZE = 0x0001;          // Retains the current size (ignores cx and cy)
        private const int SWP_NOMOVE = 0x0002;          // Retains the current position (ignores x and y)
        private const int GWL_EXSTYLE = -20;            // Index to retrieve/set the extended window style
        private const int WS_EX_TOPMOST = 0x00000008;   // Extended style: window should be placed above all non-topmost windows

        // z-order
        private const int HWND_NOTOPMOST = -2;      // Places window above all non-topmost (behind all topmost)
        private const int HWND_TOPMOST = -1;        // Places window above all non-topmost windows (Always on top)
        private const int HWND_TOP = 0;             // Places window at the top of the Z-order
        private const int HWND_BOTTOM = 1;          // Places window at the bottom of the Z-order


        // Window messages
        private const int HWND_BROADCAST = 0xffff;      // Send message to all top-level windows in the system
        private const int WM_HOTKEY = 0x0312;           // Message sent when a registered hotkey is pressed
        private const int MOD_NOREPEAT = 0x4000;        // KB Modifer: do not trigger multiple hotkey events if key is held


        private IntPtr HWindow;
        private HwndSource? HSource;
        private Dictionary<int, Shortcut> Shortcuts = [];

        private Action? OnWakeUp = null;
        private uint WakeUpMsg = 0;

        public bool DisableShortcuts = false;



        /* ------------------------------ Instance ------------------------------ */

        public Win32Window(IntPtr hwnd) 
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentNullException("hwnd cannot be null.");

            HWindow = hwnd;
            HSource = HwndSource.FromHwnd(hwnd);

            HSource.AddHook(HwndHook);
        }


        public bool AddShortcut(Shortcut shortcut)
        {
            int shortcutId = shortcut.GetId();
            if (Shortcuts.ContainsKey(shortcutId)) 
                return false;

            if (!RegisterHotKey(HWindow, shortcutId, (uint)shortcut.ModifierKeys | MOD_NOREPEAT, (uint)shortcut.PrimaryVirtualKey))
                return false;

            Shortcuts.Add(shortcutId, shortcut);
            return true;
        }


        private bool RemoveShortcut(int shortcutId)
        {
            if (!Shortcuts.ContainsKey(shortcutId)) return true;
            if (!UnregisterHotKey(HWindow, shortcutId)) return false;

            Shortcuts.Remove(shortcutId);
            return true;
        }


        public bool RemoveShortcut(Shortcut shortcut)
        {
            return RemoveShortcut(shortcut.GetId());
        }


        public void UnregisterAll()
        {
            var shortcutIds = Shortcuts.Keys.ToArray();
            foreach (var shortcutId in shortcutIds)
                RemoveShortcut(shortcutId);

            if (Shortcuts.Count > 0)
                throw new Exception("Failed to deregister all shortcuts");
        }

        
        public void RegisterWakeUpMsg(string wakeUpMsg, Action onWakeUpAction)
        {
            OnWakeUp = onWakeUpAction;
            WakeUpMsg = RegisterWindowMessage(wakeUpMsg);
        }


        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg==WM_HOTKEY)
            {
                if (!DisableShortcuts && Shortcuts.TryGetValue(wParam.ToInt32(), out var shortcut))
                {
                    Application.Current?.Dispatcher.Invoke(shortcut.OnTrigger);
                    handled = true;
                }
                return IntPtr.Zero;
            }
            
            if(msg == WakeUpMsg)
            {
                if (OnWakeUp != null)
                {
                    Application.Current?.Dispatcher.Invoke(() => { OnWakeUp.Invoke(); });
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }


        public void Dispose()
        {
            if (HSource == null) return;

            UnregisterAll();

            HSource.RemoveHook(HwndHook);
            HSource = null;
        }




        /* ------------------------------ Static ------------------------------ */

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


        public static bool BroadcastWindowMessage(string msg)
        {
            uint message = RegisterWindowMessage(msg);
            return PostMessage(HWND_BROADCAST, message, IntPtr.Zero, IntPtr.Zero);
        }


        public static IntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }


        public static bool MakeWindowTopMost(IntPtr hwnd)
        {
            return SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }


        public static bool MakeWindowNonTopMost(IntPtr hwnd)
        {
            return SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }


        public static List<WINDOWINFO> FindWindowByName(string windowName, bool startsWith = false)
        {
            var matchingWindows = new List<WINDOWINFO>();
            EnumWindows((hWnd, lParam) =>
            {
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder title = new StringBuilder(length + 1);
                GetWindowText(hWnd, title, title.Capacity);

                string wTitle = title.ToString();
                bool isMatch = startsWith ? wTitle.StartsWith(windowName) : wTitle == windowName;
                if (!isMatch) return true;

                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);

                matchingWindows.Add(new WINDOWINFO
                {
                    Handle = hWnd,
                    Title = wTitle,
                    ProcessId = processId
                });

                return true;
            },
            IntPtr.Zero);

            return matchingWindows;
        }


        public static List<WINDOWINFO> FindTopMostWindows()
        {
            var topmostWindows = new List<WINDOWINFO>();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder title = new StringBuilder(length + 1);
                GetWindowText(hWnd, title, title.Capacity);

                int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                bool isTopmost = (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;

                if (isTopmost)
                {
                    uint processId;
                    GetWindowThreadProcessId(hWnd, out processId);

                    topmostWindows.Add(new WINDOWINFO
                    {
                        Handle = hWnd,
                        Title = title.ToString(),
                        ProcessId = processId,
                        IsTopmost = true
                    });
                }

                return true;
            },
            IntPtr.Zero);

            return topmostWindows;
        }


        /* ----- Imports ----- */

        #region DLL Imports

        // ---- Shortcuts
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        // ---- Window Messages
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        // ---- Window Manipulation
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

        //-----------
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        #endregion
    }
}
