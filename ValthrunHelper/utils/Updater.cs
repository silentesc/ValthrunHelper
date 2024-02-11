using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace ValthrunHelper.utils
{
    internal class Updater
    {
        private static readonly string apiUrl = "https://api.github.com/repos/silentesc/ValthrunHelper/releases/latest";
        private static readonly string valthrunHelperUrl = "https://github.com/silentesc/ValthrunHelper/releases/latest/download/ValthrunHelper.exe";
        private static string? lastCheckedRepoVersion = null;

        public static async Task<bool> UpdateAvailableAsync()
        {
            // Get current version
            string? currentVersion = GetCurrentVersion();
            if (currentVersion == null)
            {
                MainWindow.Log("Checking versions failed - currentVersion is null");
                return false;
            }

            // Get repo version
            string? repoVersion = await GetRepoVersion();
            if (repoVersion == null)
            {
                MainWindow.Log("Checking versions failed - repoVersion is null");
                return false;
            }

            return !currentVersion.Equals(repoVersion);
        }

        public static async Task UpdateAsync()
        {
            try
            {
                // Try to get last repo version to not make a request to github again
                // If null, get repo version from github
                string? repoVersion = lastCheckedRepoVersion ?? await GetRepoVersion();
                if (repoVersion == null)
                {
                    MainWindow.Log("Update failed - repoVersion is null");
                    return;
                }

                string newValthrunHelperFileName = "ValthrunHelper " + repoVersion + ".exe";

                // Download the new file
                MainWindow.Log("Downloading new version of ValthrunHelper");
                await FileUtils.DownloadFileAsync(valthrunHelperUrl, newValthrunHelperFileName);

                // Start the new version
                Process.Start(newValthrunHelperFileName);
            }
            catch (Exception e)
            {
                MainWindow.Log("Error updating and restarting application: " + e.Message);
            }
        }

        public static void DeleteOldFiles()
        {
            // Get current process and filename of current process
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule? currentProcessModule = currentProcess.MainModule;
            if (currentProcessModule == null) return;
            string currentProcessFileName = currentProcessModule.FileName;

            // Get running processes
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                // Continue if process is current process
                if (process.Id == currentProcess.Id) continue;

                // Check Window title
                string windowTitle = process.MainWindowTitle;
                if (windowTitle != "Valthrun Helper") continue;

                // Get process file
                ProcessModule? module = process.MainModule;
                if (module == null) continue;
                string fileName = module.FileName;

                // Continue if file is the same as current file
                if (currentProcessFileName == fileName) continue;

                // Kill process
                process.Kill();

                // Wait for process to be killed
                Thread.Sleep(100);

                // Try to delete the file if exists
                try
                {
                    if (File.Exists(fileName)) File.Delete(fileName);

                }
                catch (Exception e)
                {
                    MainWindow.Log("Error while deleting old file " + e);
                }
            }

            // Get processes known as ValthrunHelper
            Process[] valthrunHelperProcesses = Process.GetProcessesByName("ValthrunHelper");

            // Handle each found ValthrunHelper process
            foreach (Process process in valthrunHelperProcesses)
            {
                // Continue if process is current process
                if (process.Id == currentProcess.Id) continue;

                // Get process file
                ProcessModule? module = process.MainModule;
                if (module == null) continue;
                string fileName = module.FileName;

                // Continue if file is the same as current file
                if (currentProcessFileName == fileName) continue;

                // Kill process
                process.Kill();

                // Wait for process to be killed
                Thread.Sleep(100);

                // Try to delete the file if exists
                try
                {
                    if (File.Exists(fileName)) File.Delete(fileName);

                }
                catch (Exception e)
                {
                    MainWindow.Log("Error while deleting old file " + e);
                }
            }
        }

        /*
         * Update Checker Utils
         */

        private static string? GetCurrentVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;

            if (version == null) return null;

            return version.ToString();
        }

        private static async Task<string?> GetRepoVersion()
        {
            // Create client
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("C# Auto Updater");

            // Send request and get response
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            // Check for unsuccessful status code
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Read response to json
            string responseBody = await response.Content.ReadAsStringAsync();
            GitHubRelease? responseJson = JsonSerializer.Deserialize<GitHubRelease>(responseBody);
            if (responseJson == null) return null;

            // Get tag from responseJson
            string? repoVersion = responseJson.tag_name;

            // Set lastCheckedRepoVersion to received repoVersion (might be null)
            lastCheckedRepoVersion = repoVersion;

            return repoVersion;
        }
        private class GitHubRelease
        {
            public string? tag_name { get; set; }
        }
    }
}
