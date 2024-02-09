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
        private static readonly string apiUrl = "https://api.github.com/repos/silentesc/ValthrunHelperFiles/releases/latest";

        public static async Task<bool> UpdateAvailableAsync()
        {
            // Get current version
            string? currentVersion = GetCurrentVersion();
            if (currentVersion == null) return false;

            // Get repo version
            string? repoVersion = await GetRepoVersionAsync();
            if (repoVersion == null) return false;

            return !currentVersion.Equals(repoVersion);
        }

        public static void Update(TextBlock textBlock)
        {
            try
            {
                // Close current application
                Process.GetCurrentProcess().Kill();

                // Delete old application files
                if (Directory.Exists(installationPath))
                {
                    Directory.Delete(installationPath, true);
                }

                FileUtils.DownloadFile(textBlock, );

                // Restart the application
                Process.Start(Path.Combine(installationPath, "YourApplication.exe"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating and restarting application: " + ex.Message);
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
