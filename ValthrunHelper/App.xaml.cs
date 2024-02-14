using System.Windows;

namespace ValthrunHelper
{
    public partial class App : Application
    {
        private Mutex? mutex;
        private static bool anotherAppStillRunning = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appMutexName = "ValthrunHelper";

            mutex = new Mutex(true, appMutexName, out bool createdNewAppMutex);

            if (!createdNewAppMutex)
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
