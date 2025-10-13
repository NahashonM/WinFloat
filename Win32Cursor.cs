
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFloat
{
    public static class Win32Cursor
    {
        
        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);


        private const int OCR_CROSS = 32515;
        private const uint OCR_NORMAL = 32512;

        private const uint SPI_SETCURSORS = 0x0057;
        private const uint SPIF_SENDCHANGE = 0x0002;


        public static bool isCrossHairActive = false;



        public static void SetGlobalCrossCursor()
        {
            if (isCrossHairActive) return;

            try
            {
                // Load the cross cursor
                IntPtr crossCursor = LoadCursor(IntPtr.Zero, (int)OCR_CROSS);
                if (crossCursor == IntPtr.Zero)
                {
                    Debug.WriteLine("Failed to load cross cursor");
                    return;
                }

                // Set system cursor to cross
                if (SetSystemCursor(crossCursor, OCR_NORMAL))
                {
                    isCrossHairActive = true;
                    return;
                }

                Debug.WriteLine("Failed to set system cursor");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting global cursor: {ex.Message}");
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
                Debug.WriteLine($"Error restoring cursor: {ex.Message}");
            }
        }
    }
}
