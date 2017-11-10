using System;
using System.Deployment.Application;
using System.IO;
using System.Reflection;

namespace MiniPie.Core {
    public sealed class AppContracts {

        public const string ClientId = "7cab801edfb04e309949c79b8e76b425";
        public const string ClientSecret = "45e1983d0ba14f2a92e6b41c93f94f98";

        public string ApplicationName => "MiniPie";

        public Version ApplicationVersion {
            get {
                try {
                    return ApplicationDeployment.IsNetworkDeployed
                               ? ApplicationDeployment.CurrentDeployment.CurrentVersion
                               : Assembly.GetEntryAssembly().GetName().Version;
                }
                catch {
                    return Assembly.GetEntryAssembly().GetName().Version;
                }
            }
        }

        public string SettingsFilename => $"{ApplicationName}.Settings.json";

        public string LogFileName => $"{ApplicationName}.log";

        public string HomepageUrl => "http://krausshq.com";

        public string SpotifyLocalHost => "http://minipie.spotilocal.com:4380/";

        public string PublisherName => "SleepyManiac";

        public string ProductName => "MiniPie";

        public string SettingsLocation {
            get {
                var location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName);
                if (!Directory.Exists(location))
                    Directory.CreateDirectory(location);
                return location;
            }
        }
    }
}