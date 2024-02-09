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
        private static readonly string valthrunHelperUrl = "https://github.com/silentesc/ValthrunHelperFiles/releases/latest/download/ValthrunHelper.exe";

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

        public static async Task Update(TextBlock textBlock)
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
                await FileUtils.DownloadFile(textBlock, valthrunHelperUrl, newValthrunHelperFileName);

                // Start the new version
                Process.Start(newValthrunHelperFileName);

                // Kill current process
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception e)
            {
                MainWindow.Log(textBlock, "Error updating and restarting application: " + e.Message);
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
