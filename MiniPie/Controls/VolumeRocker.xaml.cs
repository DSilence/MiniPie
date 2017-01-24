using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MiniPie.ViewModels;

namespace MiniPie.Controls
{
    /// <summary>
    /// Interaction logic for VolumeRocker.xaml
    /// </summary>
    public partial class VolumeRocker : UserControl
    {
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set {SetValue(VolumeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Volume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(VolumeRocker), new PropertyMetadata(new PropertyChangedCallback(VolumePropertyChanged)));
        
        public static async void VolumePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await ((VolumeRocker)d).ShellViewModel.VolumeChanged((double)e.NewValue);
        }

        public VolumeRocker()
        {
            InitializeComponent();
        }

        private ShellViewModel ShellViewModel
        {
            get { return DataContext as ShellViewModel; }
        }

        private void Menu_OnOpened(object sender, RoutedEventArgs e)
        {
            Menu.PlacementTarget = VolumeButton;
            Menu.Placement = PlacementMode.Center;

            double popupHeight = Menu.ActualHeight;
            double buttonHeight = VolumeButton.ActualHeight;
            Menu.VerticalOffset = popupHeight / 2 + buttonHeight / 2;
        }

        private void VolumeButton_Checked(object sender, RoutedEventArgs e)
        {
            Menu.IsOpen = true;
        }

        private void VolumeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Menu.IsOpen = false;
        }
    }
}
