using System;
using System.Deployment.Application;
using System.IO;
using System.Reflection;

namespace MiniPie.Core {
    public sealed class AppContracts {

        public const string ClientId = "7cab801edfb04e309949c79b8e76b425";
        public const string ClientSecret = "45e1983d0ba14f2a92e6b41c93f94f98";

        public string ApplicationName {
            get { return "MiniPie"; }
        }

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

        public string SettingsFilename
        {
            get { return string.Format("{0}.Settings.json", ApplicationName); }
        }

        public string HomepageUrl {
            get { return "http://krausshq.com"; }
        }

        public string SpotifyUrl {
            get { return "https://www.spotify.com/"; }
        }

        public string SpotifyLocalHost {
            get { return "https://minipie.spotilocal.com:4371/"; }
        }

        public string PublisherName
        {
            get { return "SleepyManiac"; }
        }

        public string ProductName
        {
            get { return "MiniPie"; }
        }

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