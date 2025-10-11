using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFloat
{
    public class KBShortcut
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public int MainKey { get; set; }


        public KBShortcut(bool control, bool shift, bool alt, int mainKey)
        {
            Control = control;
            Shift = shift;
            Alt = alt;
            MainKey = mainKey;
        }
    }
}
