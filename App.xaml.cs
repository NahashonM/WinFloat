

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
        private static string globalMutexName = $"Local\\{Assembly.GetExecutingAssembly().GetName().Name}_2025_12_00";

        protected override void OnStartup(StartupEventArgs e)
        {
            instanceMutex = new Mutex(true, globalMutexName, out bool isNewAppInstance);
            if (isNewAppInstance )
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
                instanceMutex.ReleaseMutex();
                instanceMutex.Dispose();
            }

            base.OnExit(e);
        }
    }
}
//MessageBox.Show("The application is already running.", "Instance Check");
