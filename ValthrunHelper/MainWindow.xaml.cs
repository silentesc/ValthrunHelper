using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ValthrunHelper.utils;

namespace ValthrunHelper
{
    public partial class MainWindow : Window
    {
        private static TextBlock? loggingTextBlock;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += Run;
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            loggingTextBlock = OutputTextBlock;

            Thread thread = new(() => StartProcessAsync());
            thread.IsBackground = true;
            thread.Start();
        }

        private async void StartProcessAsync()
        {
            string[] antiCheatProcesses = [
                // Riot
                "vgtray",
                "RiotClientUxRender",
                "RiotClientCrashHandler",
                "RiotClientUx",
                "RiotClientServices",
                // Epic Games
                "EpicWebHelper",
                "EpicGamesLauncher",
                // EasyAntiCheat
                "EasyAntiCheat",
                // BattleEye
                "BEService"
            ];
            string cs2ProcessName = "cs2";
            Process cs2Process;

            // Check for update
            if (await Updater.UpdateAvailableAsync())
            {
                await Updater.UpdateAsync();
                Log("Downloaded update, waiting for being deleted (Don't close me)");
                return;
            }

            // Check for old Valthrun processes to kill
            Updater.DeleteOldFiles();

            // Wait for AntiCheat closing
            CheatUtils.WaitForAntiCheatClosing(antiCheatProcesses);

            // Download files if they don't exist
            await CheatUtils.CheckOrDownloadFilesAsync();

            // Wait for cs2 to start and get cs2Process
            Log("Waiting for cs2 to start...");
            cs2Process = CheatUtils.WaitForCs2Process(cs2ProcessName);

            // Start cheat
            Log("Starting cheat");
            Process controllerProcess = CheatUtils.StartCheat();

            // Hide window
            Application.Current.Dispatcher.Invoke(() =>
            {
                Visibility = Visibility.Collapsed;
            });

            // Wait until cs2 closes
            cs2Process.WaitForExit();
            Application.Current.Dispatcher.Invoke(() =>
            {
                new RestartInfoWindow().Show();
            });

            // Wait until controller closes
            controllerProcess.WaitForExit();

            // Delete files
            CheatUtils.DeleteCheatFiles();
        }

        public static void Log(string message)
        {
            if (loggingTextBlock == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                string messages = loggingTextBlock.Text;
                if (messages.Length > 0) messages += Environment.NewLine;
                messages += message;
                loggingTextBlock.Text = messages;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheatUtils.DeleteCheatFiles();
        }
    }
}
