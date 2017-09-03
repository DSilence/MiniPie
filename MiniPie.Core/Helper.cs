using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace MiniPie.Core
{
    public static class Helper
    {
        public static void OpenUrl(string url)
        {
            Process.Start(url);
        }

        public static string MakeNiceSize(double size)
        {
            var suffix = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var run = 0;
            while (size >= 1024)
            {
                size /= 1024;
                run++;
            }
            return Math.Round(size, 2).ToString("0.00") + " " + suffix[run];
        }

        public static ImageSource GetImageSourceFromResource(string psResourceName)
        {
            try
            {
                var oUri = new Uri("pack://application:,,,/MiniPie;component/" + psResourceName,
                    UriKind.RelativeOrAbsolute);
                return BitmapFrame.Create(oUri);
            }
            catch (FileFormatException)
            {
                return null;
            }
        }

        public static Task<string> SerializeObjectAsync<T>(T value)
        {
            return Task.Run(() => JsonConvert.SerializeObject(value));
        }

        public static Task SerializeToStreamAsync<T>(T value, Stream stream)
        {
            return Task.Run(() =>
            {
                var sw = new StreamWriter(stream);
                var serializer = new JsonSerializer();
                serializer.Serialize(sw, value);
                sw.Flush();
            });
        }

        public static Task<T> DeserializeStringAsync<T>(string value)
        {
            return Task.Run(() => JsonConvert.DeserializeObject<T>(value));
        }

        public static Task<T> DeserializeStreamAsync<T>(Stream input)
        {
            return Task.Run(() =>
            {
                using (StreamReader sr = new StreamReader(input))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();

                    // read the json from a stream
                    // json size doesn't matter because only a small piece is read at a time from the HTTP request
                    var result = serializer.Deserialize<T>(reader);
                    return result;
                }
            });
        }
    }
}