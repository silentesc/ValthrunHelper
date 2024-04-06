using Octokit;
using System.IO;
using System.Net.Http;

namespace ValthrunHelper.utils
{
    internal class FileUtils
    {
        public static async Task DownloadFileAsync(string url, string destinationPath)
        {
            HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                MainWindow.Log(string.Format("Failed to download from {0} - Status code: {1}", url, response.StatusCode));
                return;
            }

            HttpContent content = response.Content;
            byte[] data = await content.ReadAsByteArrayAsync();
            File.WriteAllBytes(destinationPath, data);
        }

        public static async Task DownloadController(string baseUrl, string destinationPath)
        {
            var github = new GitHubClient(new ProductHeaderValue("ValthrunHelper"));
            var repoOwner = "Valthrun";
            var repoName = "Valthrun";

            try
            {
                // Get the latest release
                var latestRelease = await github.Repository.Release.GetLatest(repoOwner, repoName);

                // Get assets associated with the latest release
                var assets = await github.Repository.Release.GetAllAssets(repoOwner, repoName, latestRelease.Id);

                if (!assets.Any())
                {
                    MainWindow.Log($"No assets found in the latest release of {repoOwner}/{repoName}");
                    return;
                }

                foreach (var asset in assets)
                {
                    if (asset.Name.Contains("controller", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await DownloadFileAsync(baseUrl + asset.Name, destinationPath);
                        return;
                    }
                }
            }
            catch (NotFoundException)
            {
                MainWindow.Log($"Repository '{repoOwner}/{repoName}' not found.");
            }
        }
    }
}
