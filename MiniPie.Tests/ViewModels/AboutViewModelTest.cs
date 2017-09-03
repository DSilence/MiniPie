using MiniPie.Core;
using MiniPie.ViewModels;
using Xunit;

namespace MiniPie.Tests.ViewModels
{
    public class AboutViewModelTest
    {
        [Fact]
        public void TestAboutViewModel()
        {
            var aboutViewModel = new AboutViewModel(new AppContracts());
            Assert.Collection(aboutViewModel.UsedComponents,
                (i) =>
                {
                    Assert.Equal("http://caliburnmicro.codeplex.com/", i.Url);
                    Assert.Equal("MIT License", i.License);
                    Assert.Equal("Caliburn.Micro", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("http://nlog-project.org/", i.Url);
                    Assert.Equal("MIT License", i.License);
                    Assert.Equal("NLog", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("http://json.codeplex.com/", i.Url);
                    Assert.Equal("MIT License", i.License);
                    Assert.Equal("Newtonsoft Json.Net", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("http://jariz.nl", i.Url);
                    Assert.Equal("Apache 2.0 License", i.License);
                    Assert.Equal("Spotify local API", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("http://www.codeproject.com/Articles/35159/WPF-Localization-Using-RESX-Files", i.Url);
                    Assert.Equal("CPOL", i.License);
                    Assert.Equal("Resx Extention", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("https://github.com/Fody/Fody", i.Url);
                    Assert.Equal("MIT", i.License);
                    Assert.Equal("Fody", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("http://www.codeproject.com/Articles/36468/WPF-NotifyIcon", i.Url);
                    Assert.Equal("CPOL", i.License);
                    Assert.Equal("Hardcodet WPF NotifyIcon", i.Name);
                },
                (i) =>
                {
                    Assert.Equal("https://github.com/Squirrel/Squirrel.Windows", i.Url);
                    Assert.Equal("Github license", i.License);
                    Assert.Equal("Squirrel.Windows", i.Name);
                }
                );
            Assert.Equal("MiniPie", aboutViewModel.ApplicationName);
        }
    }
}
