
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;

namespace WinFloat
{
    internal static class AppConfig
    {
        public static Shortcut? PinShortcut;
        public static Shortcut? UnPinShortcut;

        public static bool MoveToTrayOnExit;
        public static bool CanSelectAppWindow;

        private static Action? PinShortcutAction;
        private static Action? UnPinShortcutAction;

        private static string ConfigFileName = "config.cfg";


        public static void Initialize(Action pinShortcutAction, Action unPinShortcutAction)
        {
            PinShortcutAction = pinShortcutAction;
            UnPinShortcutAction = unPinShortcutAction;
        }

        public static void ResetToDefault()
        {
            if (PinShortcutAction == null || UnPinShortcutAction == null)
                throw new Exception("Config not initialized.");

            PinShortcut= new Shortcut(
                Shortcut.MOD_CONTROL | Shortcut.MOD_ALT | Shortcut.MOD_SHIFT, 
                Key.P, PinShortcutAction);

            UnPinShortcut = new Shortcut(
                Shortcut.MOD_CONTROL | Shortcut.MOD_ALT | Shortcut.MOD_SHIFT,
                Key.U, UnPinShortcutAction);

            MoveToTrayOnExit = true;
            CanSelectAppWindow = false;
        }


        private static string ResolveConfigFolder()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appName = Assembly.GetExecutingAssembly().GetName().Name;

            string appConfigFolder = Path.Combine(localAppData, appName);

            if(!File.Exists(appConfigFolder))
                Directory.CreateDirectory(appConfigFolder);

            return appConfigFolder;
        }


        public static bool LoadFromStorage()
        {
            if (PinShortcutAction == null || UnPinShortcutAction == null)
                throw new Exception("Config not initialized.");

            string cfgPath = Path.Combine(ResolveConfigFolder(), ConfigFileName);
            if (!File.Exists(cfgPath)) return false;

            string json = File.ReadAllText(cfgPath);
            Dictionary<string, string> config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (config == null) return false;

            if (config.TryGetValue("MoveToTrayOnExit", out string moveToTrayValue)) MoveToTrayOnExit = bool.Parse(moveToTrayValue);
            if (config.TryGetValue("CanSelectAppWindow", out string canSelectAppValue)) CanSelectAppWindow = bool.Parse(canSelectAppValue);
            if (config.TryGetValue("PinShortcut", out string pinStr)) PinShortcut = new Shortcut(pinStr, PinShortcutAction);
            if (config.TryGetValue("UnPinShortcut", out string unpinStr)) UnPinShortcut = new Shortcut(unpinStr, UnPinShortcutAction);

            return true;
        }


        public static bool SaveToStorage() {
            if (PinShortcutAction == null || UnPinShortcutAction == null)
                throw new Exception("Config not initialized.");

            if (PinShortcut == null || UnPinShortcut == null) return false;

            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add("PinShortcut", PinShortcut.ToString());
            config.Add("UnPinShortcut", UnPinShortcut.ToString());
            config.Add("MoveToTrayOnExit", MoveToTrayOnExit.ToString());
            config.Add("CanSelectAppWindow", CanSelectAppWindow.ToString());

            string jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            string cfgPath = Path.Combine(ResolveConfigFolder(), ConfigFileName);

            File.WriteAllText(cfgPath, jsonContent);

            return true; 
        }   
    }
}
