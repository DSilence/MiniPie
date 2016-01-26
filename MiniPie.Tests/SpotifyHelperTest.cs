using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniPie.Core;
using MiniPie.Core.SpotifyWeb.Models;
using Newtonsoft.Json;
using Xunit;

namespace MiniPie.Tests
{
    public class SpotifyHelperTest
    {
        public SpotifyHelperTest()
        {
            //Register uro scheme "pack"
            if (!UriParser.IsKnownScheme("pack"))
                new System.Windows.Application();
        }

        [Fact]
        public async void AsyncDeserializeTest()
        {
            ExternalUrl test = new ExternalUrl
            {
                Key = "abc",
                Value = "qwe"
            };
            var testSerialized = await Helper.SerializeObjectAsync(test);
            var resulting = await Helper.DeserializeStringAsync<ExternalUrl>(testSerialized);
            Assert.Equal(test.Key, resulting.Key);
            Assert.Equal(test.Value, resulting.Value);
        }

        [Fact]
        public void TestGetImageFromResource()
        {
            var icon = Helper.GetImageSourceFromResource("App.ico");
            Assert.NotNull(icon);
            Assert.Throws<IOException>(() =>
            {
                var fake = Helper.GetImageSourceFromResource("fake");
            });
        }
    }
}
