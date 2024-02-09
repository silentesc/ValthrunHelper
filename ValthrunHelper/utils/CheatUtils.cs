using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace ValthrunHelper.utils
{
    internal class CheatUtils
    {
        private static readonly string filesPath = "files";

        private static readonly string kdmapperPath = string.Format("{0}\\{1}", filesPath, "kdmapper.exe");
        private static readonly string controllerPath = string.Format("{0}\\{1}", filesPath, "controller.exe");
        private static readonly string valthrunDriverPath = string.Format("{0}\\{1}", filesPath, "valthrun-driver.sys");
        private static readonly string vulkan1Path = string.Format("{0}\\{1}", filesPath, "vulkan-1.dll");
        private static readonly string configPath = string.Format("{0}\\{1}", filesPath, "config.yaml");

        private static readonly string kdmapperURL = "https://github.com/valthrunner/Valthrun/releases/latest/download/kdmapper.exe";
        private static readonly string controllerURL = "https://github.com/Valthrun/Valthrun/releases/latest/download/controller.exe";
        private static readonly string valthrunDriverURL = "https://github.com/Valthrun/Valthrun/releases/latest/download/valthrun-driver.sys";
        private static readonly string vulkan1URL = "https://github.com/silentesc/ValthrunHelperFiles/releases/latest/download/vulkan-1.dll";
        private static readonly string configURL = "https://github.com/silentesc/ValthrunHelperFiles/releases/latest/download/config.yaml";

        public static void WaitForAntiCheatClosing(TextBlock outputTextBlock, string[] antiCheatProcesses)
        {
            // Check for AntiCheat Processes and warn user if so
            bool alreadyLogged = false;
            while (true)
            {
                // Check if AntiCheat Processes found
                List<string> foundAntiCheatProcesses = GetRunningAntiCheatProcess(antiCheatProcesses);
                if (foundAntiCheatProcesses.Count <= 0) break;

                // Print message and clear list
                if (!alreadyLogged)
                {
                    alreadyLogged = true;
                    MainWindow.Log(outputTextBlock, "The following processes have to be closed to start the cheat:");
                    MainWindow.Log(outputTextBlock, string.Join(", ", foundAntiCheatProcesses));
                }
                foundAntiCheatProcesses.Clear();

                // Sleep
                Thread.Sleep(1000);
            }
        }

        public static void CheckOrDownloadFiles(TextBlock outputTextBlock)
        {
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            // Download kdmapper.exe if doesn't exist
            if (!File.Exists(kdmapperPath))
            {
                MainWindow.Log(outputTextBlock, "Downloading kdmapper.exe");
                FileUtils.DownloadFile(outputTextBlock, kdmapperURL, kdmapperPath);
            }

            // Download controller.exe if doesn't exist
            if (!File.Exists(controllerPath))
            {
                MainWindow.Log(outputTextBlock, "Downloading controller.exe");
                FileUtils.DownloadFile(outputTextBlock, controllerURL, controllerPath);
            }

            // Download valthrun-driver.sys if doesn't exist
            if (!File.Exists(valthrunDriverPath))
            {
                MainWindow.Log(outputTextBlock, "Downloading valthrun-driver.sys");
                FileUtils.DownloadFile(outputTextBlock, valthrunDriverURL, valthrunDriverPath);
            }

            // Download vulkan-1.dll if doesn't exist
            if (!File.Exists(vulkan1Path))
            {
                MainWindow.Log(outputTextBlock, "Downloading vulkan-1.dll");
                FileUtils.DownloadFile(outputTextBlock, vulkan1URL, vulkan1Path);
            }

            // Download config.yaml if doesn't exist
            if (!File.Exists(configPath))
            {
                MainWindow.Log(outputTextBlock, "Downloading config.yaml");
                FileUtils.DownloadFile(outputTextBlock, configURL, configPath);
            }
        }

        public static Process WaitForCs2Process(string cs2ProcessName)
        {
            while (true)
            {
                // Check for cs2 process
                Process[] cs2Processes = Process.GetProcessesByName(cs2ProcessName);

                if (cs2Processes.Length > 0)
                {
                    Process cs2Process = cs2Processes[0];

                    // Wait for window to create
                    while (cs2Process.MainWindowHandle == IntPtr.Zero)
                    {
                        Thread.Sleep(1000);
                    }

                    return cs2Process;
                }

                // Sleep
                Thread.Sleep(1000);
            }
        }

        public static Process StartCheat()
        {
            // Loading driver
            ProcessStartInfo driverProcessStartInfo = new()
            {
                FileName = kdmapperPath,
                Arguments = valthrunDriverPath,
                Verb = "runas",
                UseShellExecute = true
            };
            Process driverProcess = new() { StartInfo = driverProcessStartInfo };
            driverProcess.Start();
            driverProcess.WaitForExit();

            // Load controller
            ProcessStartInfo controllerProcessStartInfo = new()
            {
                FileName = controllerPath,
                UseShellExecute = true
            };
            Process controllerProcess = new() { StartInfo = controllerProcessStartInfo };
            controllerProcess.Start();

            return controllerProcess;
        }

        public static void DeleteCheatFiles()
        {
            if (!Directory.Exists(filesPath)) return;

            try
            {
                if (File.Exists(kdmapperPath)) File.Delete(kdmapperPath);
                if (File.Exists(controllerPath)) File.Delete(controllerPath);
                if (File.Exists(valthrunDriverPath)) File.Delete(valthrunDriverPath);
                if (File.Exists(vulkan1Path)) File.Delete(vulkan1Path);
            }
            catch { }
        }

        private static List<string> GetRunningAntiCheatProcess(string[] antiCheatProcesses)
        {
            Process[] runningProcesses = Process.GetProcesses();
            List<string> foundAntiCheatProcesses = [];

            // Add AntiCheat Processes to foundAntiCheatProcesses
            foreach (Process process in runningProcesses)
            {
                string processName = process.ProcessName;
                if (antiCheatProcesses.Contains(processName))
                {
                    foundAntiCheatProcesses.Add(processName);
                }
            }

            return foundAntiCheatProcesses;
        }
    }
}
