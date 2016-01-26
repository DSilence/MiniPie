using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public static bool IsWindows7
        {
            get { return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1; }
        }

        public static string MakeNiceSize(double size)
        {
            return MakeNiceSize(size, "auto");
        }

        private static string MakeNiceSize(double size, string mode)
        {
            var suffix = new[] {"B", "KB", "MB", "GB", "TB", "PB", "EB"};
            var run = 0;

            if (mode == "auto")
            {
                while (size >= 1024)
                {
                    size /= 1024;
                    run++;
                }
            }
            else if (mode != "auto")
            {
                if (suffix.Contains(mode))
                {
                    while (suffix[run] != mode)
                    {
                        size /= 1024;
                        run++;
                    }
                }
                else
                {
                    return "ERROR: Unknown mode";
                }
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