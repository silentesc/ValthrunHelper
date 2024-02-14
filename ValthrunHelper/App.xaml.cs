using System.Windows;

namespace ValthrunHelper
{
    public partial class App : Application
    {
        private Mutex? mutex;
        private static bool anotherAppStillRunning = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "YourUniqueMutexName"; // Choose a unique name for your mutex

            mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                anotherAppStillRunning = true;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            base.OnExit(e);
        }

        public static bool AnotherAppStillRunning()
        {
            return anotherAppStillRunning;
        }
    }
}
