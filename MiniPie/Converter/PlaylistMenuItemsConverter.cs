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

namespace MiniPie.Converter
{
    public class PlaylistMenuItemsConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlists = value as ObservableCollection<Playlist>;
            if (playlists == null)
            {
                return null;
            }
            var result = new List<MenuItem>(playlists.Count);
            foreach (var playlist in playlists)
            {
                
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
