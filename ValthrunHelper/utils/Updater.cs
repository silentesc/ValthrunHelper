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
        private static readonly string valthrunHelperFileName = "ValthrunHelper.exe";

        public static async Task<bool> UpdateAvailableAsync(TextBlock textBlock)
        {
            // Get current version
            string? currentVersion = GetCurrentVersion();
            if (currentVersion == null)
            {
                MainWindow.Log(textBlock, "currentVersion is null");
                return false;
            }

            // Get repo version
            string? repoVersion = await GetRepoVersionAsync();
            if (repoVersion == null)
            {
                MainWindow.Log(textBlock, "repoVersion is null");
                return false;
            }

            return !currentVersion.Equals(repoVersion);
        }

        public static async Task Update(TextBlock textBlock)
        {
            try
            {
                // Close current application
                Process.GetCurrentProcess().Kill();

                // Delete old exe
                if (File.Exists(valthrunHelperFileName))
                {
                    File.Delete(valthrunHelperFileName);
                }

                // Download the new file
                await FileUtils.DownloadFile(textBlock, valthrunHelperUrl, "");

                // Restart the application
                Process.Start(valthrunHelperFileName);
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

        private static async Task<string?> GetRepoVersionAsync()
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
