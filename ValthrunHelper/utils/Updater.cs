using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows.Controls;

namespace ValthrunHelper.utils
{
    internal class Updater
    {
        private static readonly string apiUrl = "https://api.github.com/repos/silentesc/ValthrunHelper/releases/latest";
        private static readonly string valthrunHelperUrl = "https://github.com/silentesc/ValthrunHelper/releases/latest/download/ValthrunHelper.exe";

        public static async Task<bool> UpdateAvailableAsync(TextBlock textBlock)
        {
            // Get current version
            string? currentVersion = GetCurrentVersion();
            if (currentVersion == null)
            {
                MainWindow.Log(textBlock, "Checking versions failed - currentVersion is null");
                return false;
            }

            // Get repo version
            string? repoVersion = await GetRepoVersion();
            if (repoVersion == null)
            {
                MainWindow.Log(textBlock, "Checking versions failed - repoVersion is null");
                return false;
            }

            return !currentVersion.Equals(repoVersion);
        }

        public static async Task UpdateAsync(TextBlock textBlock)
        {
            try
            {
                // Get repo version
                string? repoVersion = await GetRepoVersion();
                if (repoVersion == null)
                {
                    MainWindow.Log(textBlock, "Update failed - repoVersion is null");
                    return;
                }

                string newValthrunHelperFileName = "ValthrunHelper " + repoVersion + ".exe";

                // Download the new file
                MainWindow.Log(textBlock, "Downloading new version of ValthrunHelper");
                await FileUtils.DownloadFileAsync(textBlock, valthrunHelperUrl, newValthrunHelperFileName);

                // Start the new version
                Process.Start(newValthrunHelperFileName);
            }
            catch (Exception e)
            {
                MainWindow.Log(textBlock, "Error updating and restarting application: " + e.Message);
            }
        }

        public static void DeleteOldFiles()
        {
            int currentProcessId = Environment.ProcessId;
            Process[] valthrunHelperProcesses = Process.GetProcessesByName("ValthrunHelper");

            foreach (Process process in valthrunHelperProcesses)
            {
                // Continue if process is current process
                if (process.Id == currentProcessId) continue;

                // Get process file
                ProcessModule? module = process.MainModule;
                if (module == null) continue;

                // Kill process
                process.Kill();

                // Try to delete the file if exists
                try
                {
                    if (File.Exists(module.FileName)) File.Delete(module.FileName);
                }
                catch { }
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
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("C# Auto Updater");

            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            GitHubRelease? responseJson = JsonSerializer.Deserialize<GitHubRelease>(responseBody);
            if (responseJson == null) return null;

            return responseJson.tag_name;
        }
        private class GitHubRelease
        {
            public string? tag_name { get; set; }
        }
    }
}
