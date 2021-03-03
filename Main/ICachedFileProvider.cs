using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Graphics;
using Path = System.IO.Path;

namespace Madamin.Unfollow.Main
{
    public interface ICacheProvider
    {
        Task FetchIfRequired(string url);
        Task<Bitmap> ReadBitmapFromCache(string url);
    }

    public partial class MainActivity : ICacheProvider
    {
        private HttpClient _client;

        private string GetCacheFile(Uri uri)
        {
            var cacheDir = ((IDataStorage)this).GetCacheDir();
            var fileName = Path.GetFileName(uri.LocalPath);
            return Path.Combine(cacheDir, fileName);
        }

        async Task ICacheProvider.FetchIfRequired(string url)
        {
            var uri = new Uri(url);
            var cacheFilePath = GetCacheFile(uri);
            if (File.Exists(cacheFilePath))
                return;
            var response = await _client.GetAsync(uri);
            var stream = await response.Content.ReadAsStreamAsync();
            var cacheFile = new FileStream(cacheFilePath, FileMode.CreateNew);
            await stream.CopyToAsync(cacheFile);
        }

        Task<Bitmap> ICacheProvider.ReadBitmapFromCache(string url)
        {
            var uri = new Uri(url);
            return BitmapFactory.DecodeFileAsync(GetCacheFile(uri));
        }
    }
}