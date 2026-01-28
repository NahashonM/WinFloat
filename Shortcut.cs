
using System.Windows.Input;


namespace WinFloat
{
    internal class Shortcut
    {
        public static List<Key> AllowedModifierKeys = [Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.LWin, Key.RWin];

        private static Action NoOp = () => { };

        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        public Key PrimaryKey { get; private set; }
        public int ModifierKeys { get; private set; }
        public int PrimaryVirtualKey { get; private set; }

        public Action OnTrigger { get; private set; }

        /* ------ Constructors ------ */
        private Shortcut((int mods, Key k) data, Action onTrigger) : this(data.mods, data.k, onTrigger) { }

        public Shortcut(string keyCombo) : this(ParseFromString(keyCombo), NoOp) { }
        public Shortcut(int modifierKeys, Key key) : this(modifierKeys, key, NoOp) { }
        public Shortcut(string keyCombo, Action onTrigger) : this(ParseFromString(keyCombo), onTrigger) { }

        public Shortcut(int modifierKeys, Key key, Action onTrigger)
        {
            PrimaryKey = key;
            ModifierKeys = modifierKeys;
            OnTrigger = onTrigger ?? NoOp;
            PrimaryVirtualKey = KeyInterop.VirtualKeyFromKey(PrimaryKey);
        }
        /* ------ End Constructors ------ */


        public int GetId()
        {
            return ((int)ModifierKeys << 16) | (int)PrimaryKey;
        }


        public override string ToString()
        {
            var modifiers = new List<string>();

            if ((ModifierKeys & MOD_CONTROL) == MOD_CONTROL)
                modifiers.Add("Ctrl");
            if ((ModifierKeys & MOD_ALT) == MOD_ALT)
                modifiers.Add("Alt");
            if ((ModifierKeys & MOD_SHIFT) == MOD_SHIFT)
                modifiers.Add("Shift");
            if ((ModifierKeys & MOD_WIN) == MOD_WIN)
                modifiers.Add("Win");

            modifiers.Add(PrimaryKey.ToString());

            return string.Join(" + ", modifiers);
        }


        private static (int modifiers, Key key) ParseFromString(string combo)
        {
            int mods = 0;
            Key k = Key.None;

            if (string.IsNullOrEmpty(combo))
                return (mods, k);

            string[] parts = combo.Split([" + "], StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                string p = part.Trim();
                switch (p)
                {
                    case "Ctrl": mods |= MOD_CONTROL; break;
                    case "Shift": mods |= MOD_SHIFT; break;
                    case "Alt": mods |= MOD_ALT; break;
                    case "Win": mods |= MOD_WIN; break;
                    default:
                        if (Enum.TryParse(p, out Key parsedKey))
                            k = parsedKey;
                        break;
                }
            }

            return (mods, k);
        }
    }
}
