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
    }
}
