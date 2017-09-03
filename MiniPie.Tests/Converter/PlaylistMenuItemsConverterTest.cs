using MiniPie.Converter;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MiniPie.Core.SpotifyWeb.Models;
using Xunit;
using NSubstitute;
using MiniPie.ViewModels;
using MiniPie.Core;

namespace MiniPie.Tests.Converter
{
    public class PlaylistMenuItemsConverterTest
    {
        private PlaylistMenuItemsConverter _playlistConverter = new PlaylistMenuItemsConverter();

        [Fact(DisplayName = "Can convert")]
        public async Task ConvertTest()
        {
            var playlists = new ObservableCollection<Playlist>
            {
                new Playlist
                {
                    Name = "Test1"
                },
                new Playlist
                {
                    Name = "Test2"
                }
            };
            var model = Substitute.For<ShellViewModel>(null, Substitute.For<ISpotifyController>(), null, Substitute.For<AppSettings>(), null, null, null);
            Assert.Null(_playlistConverter.Convert(new[] { playlists, null}, null, null, null));
            Assert.Null(_playlistConverter.Convert(new[] { null, model }, null, null, null));

            var result = _playlistConverter.Convert(new object[] { playlists, model }, null, null, null);
            var typedResult = result as ObservableCollection<PlaylistItemViewModel>;
            Assert.NotNull(typedResult);
            Assert.Equal(typedResult[0].Name, "Test1");
            Assert.Equal(typedResult[1].Name, "Test2");

            typedResult[0].Action();
            await model.ReceivedWithAnyArgs(1).AddToPlaylist("");
        }

        [Fact(DisplayName = "Can convert back")]
        public void ConvertBackTest()
        {
            Assert.Throws<NotImplementedException>(() => _playlistConverter.ConvertBack(null, null, null, null));
        }
    }
}
