using System;
using System.Collections.Generic;
using Caliburn.Micro;
using MiniPie.Core;

namespace MiniPie.ViewModels {
    public sealed class AboutViewModel : Screen {
        private readonly AppContracts _Contracts;

        public class ComponentData {
            public ComponentData(string url, string license, string name) {
                Url = url;
                License = license;
                Name = name;
            }

            public string Name { get; set; }
            public string License { get; set; }
            public string Url { get; set; }
        }

        public AboutViewModel(AppContracts contracts) {
            _Contracts = contracts;

            DisplayName = string.Format("About - {0}", _Contracts.ApplicationName);
            UsedComponents = new List<ComponentData>(new [] {
                                                                new ComponentData("http://caliburnmicro.codeplex.com/","MIT License","Caliburn.Micro"),
                                                                new ComponentData("http://nlog-project.org/","MIT License", "NLog"), 
                                                                new ComponentData("http://json.codeplex.com/","MIT License", "Newtonsoft Json.Net"), 
                                                                new ComponentData("http://jariz.nl", "Apache 2.0 License","Spotify local API"),
                                                                new ComponentData("http://www.codeproject.com/Articles/35159/WPF-Localization-Using-RESX-Files", "CPOL", "Resx Extention"), 
                                                                new ComponentData("https://github.com/Fody/Fody", "MIT", "Fody"), 
                                                                new ComponentData("http://www.codeproject.com/Articles/36468/WPF-NotifyIcon", "CPOL", "Hardcodet WPF NotifyIcon"),
                                                                new ComponentData("https://github.com/Squirrel/Squirrel.Windows", "Github license", "Squirrel.Windows"), 
                                                            });
        }

        public string ApplicationName => _Contracts.ApplicationName;
        public Version ApplicationVersion => _Contracts.ApplicationVersion;

        public List<ComponentData> UsedComponents { get; set; }

        public ComponentData SelectedComponent { get; set; }

        public void GoHome()
        {
            Helper.OpenUrl(_Contracts.HomepageUrl);
        }

        public void OpenComponentUrl()
        {
            Helper.OpenUrl(SelectedComponent.Url);
        }

    }
}
