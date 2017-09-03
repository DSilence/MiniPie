using System;
using System.IO;
using System.Threading.Tasks;
using MiniPie.Core;
using MiniPie.Core.SpotifyWeb.Models;
using Xunit;
using System.Windows;

namespace MiniPie.Tests
{
    public class SpotifyHelperTest
    {
        public SpotifyHelperTest()
        {
            //Register uri scheme "pack"
            if (!UriParser.IsKnownScheme("pack"))
            {
                var app = new Application();
            }
        }

        [Theory]
        [InlineData(2010200, "1.92 MB")]
        [InlineData(2, "2.00 B")]
        [InlineData(100000, "97.66 KB")]
        public void MakeNiceSizeTest(double input, string expected)
        {
            Assert.Equal(expected, Helper.MakeNiceSize(input));
        }

        [Fact]
        public async Task AsyncDeserializeTest()
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
