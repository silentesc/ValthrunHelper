using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ValthrunHelper.utils;

namespace ValthrunHelper
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += Run;
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            Thread thread = new(() => StartProcessAsync(OutputTextBlock));
            thread.IsBackground = true;
            thread.Start();
        }

        private async void StartProcessAsync(TextBlock textBlock)
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
            if (await Updater.UpdateAvailableAsync(textBlock))
            {
                await Updater.Update(textBlock);
                return;
            }

            return;

            // Wait for AntiCheat closing
            CheatUtils.WaitForAntiCheatClosing(textBlock, antiCheatProcesses);

            // Download files if they don't exist
            await CheatUtils.CheckOrDownloadFiles(textBlock);

            // Wait for cs2 to start and get cs2Process
            Log(textBlock, "Waiting for cs2 to start...");
            cs2Process = CheatUtils.WaitForCs2Process(cs2ProcessName);

            // Start cheat
            Log(textBlock, "Starting cheat");
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

        public static void Log(TextBlock textBlock, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string messages = textBlock.Text;
                if (messages.Length > 0) messages += Environment.NewLine;
                messages += message;
                textBlock.Text = messages;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheatUtils.DeleteCheatFiles();
        }
    }
}
