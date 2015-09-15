using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using MiniPie.Core.SpotifyWeb.Models;
using MiniPie.ViewModels;

namespace MiniPie.Converter
{
    public class PlaylistMenuItemsConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var playlists = values[0] as ObservableCollection<Playlist>;
            var viewModel = values[1] as ShellViewModel;
            if (playlists == null || viewModel == null)
            {
                return null;
            }
            var result = new List<MenuItem>(playlists.Count);
            foreach (var playlist in playlists)
            {
                var menuItem = new MenuItem
                {
                    Header = playlist.Name,
                };
                menuItem.Click += (sender, args) =>
                {
                    viewModel.AddToPlaylist(playlist.Id);
                };
                result.Add(menuItem);
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
