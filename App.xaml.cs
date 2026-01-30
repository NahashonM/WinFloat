

using System.Reflection;
using System.Windows;

using Application = System.Windows.Application;


namespace WinFloat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string WakeUpMsg = "WinFloatWakeUp";

        private static Mutex? instanceMutex = null;
        private static string localMutexName = $"Local\\{Assembly.GetExecutingAssembly().GetName().Name}_2025_12_00";
        private static bool   ownsMutex = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            instanceMutex = new Mutex(true, localMutexName, out ownsMutex);
            if (ownsMutex)
            {
                base.OnStartup(e);
                return;
            }

            Win32Window.BroadcastWindowMessage(WakeUpMsg);
            Current.Shutdown();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            if(instanceMutex != null)
            {
                if (ownsMutex)
                {
                    try
                    {
                        instanceMutex.ReleaseMutex();
                    }
                    catch (Exception ex) {}
                }
                instanceMutex.Dispose();
            }

            base.OnExit(e);
        }
    }
}
