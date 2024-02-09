using System.IO;
using System.Net.Http;
using System.Windows.Controls;

namespace ValthrunHelper.utils
{
    internal class FileUtils
    {
        public static async void DownloadFile(TextBlock outputTextBlock, string url, string destinationPath)
        {
            HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                MainWindow.Log(outputTextBlock, string.Format("Failed to download from {0} - Status code: {1}", url, response.StatusCode));
                return;
            }

            HttpContent content = response.Content;
            byte[] data = await content.ReadAsByteArrayAsync();
            File.WriteAllBytes(destinationPath, data);
        }
    }
}
