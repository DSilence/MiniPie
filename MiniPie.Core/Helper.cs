using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace MiniPie.Core {
    public static class Helper {

        public static void OpenUrl(string url) {
            try {
                Process.Start(url);
            }
            catch (Exception) {
                MessageBox.Show(string.Format("Failed to open your default browser. MiniPie tried to open the following url for you: {0}", url),
                    "MiniPie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static bool IsWindows7 {
            get { return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1; }
        }

        public static string MakeNiceSize(double size) {
            return MakeNiceSize(size, "auto");
        }

        private static string MakeNiceSize(double size, string mode) {
            var suffix = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var run = 0;

            if (mode == "auto") {
                while (size >= 1024) {
                    size /= 1024;
                    run++;
                }
            }
            else if (mode != "auto") {
                if (suffix.Contains(mode)) {
                    while (suffix[run] != mode) {
                        size /= 1024;
                        run++;
                    }
                }
                else {
                    return "ERROR: Unknown mode";
                }

            }
            return Math.Round(size, 2).ToString("0.00") + " " + suffix[run];
        }

        public static ImageSource GetImageSourceFromResource(string psResourceName) {
            try {
                var oUri = new Uri("pack://application:,,,/MiniPie;component/" + psResourceName, UriKind.RelativeOrAbsolute);
                return BitmapFrame.Create(oUri);
            }
            catch (FileFormatException) { return null; }
        }

        public static Task<T> DeserializeObjectAsync<T>(string value)
        {
            return Task.Run(() => JsonConvert.DeserializeObject<T>(value));
        }
    }
}
