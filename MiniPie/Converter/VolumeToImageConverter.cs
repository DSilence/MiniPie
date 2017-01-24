using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MiniPie.Converter
{
    public class VolumeToImageConverter: IMultiValueConverter
    {
        public Canvas HighCanvas { get; set; }
        public Canvas MediumCanvas { get; set; }
        public Canvas LowCanvas { get; set; }
        public Canvas MutedCanvas { get; set; }
        public SolidColorBrush NormalBrush { get; set; }
        public SolidColorBrush MouseoverBrush { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double valuePercent = (double) values[0] * 100;
            bool isMouseOver = (bool)values[1];
            bool isChecked = (bool) values[2];

            SolidColorBrush brush = isMouseOver || isChecked ? MouseoverBrush : NormalBrush;
            ((Path) (HighCanvas.Children[0])).Fill = brush;
            ((Path)(MediumCanvas.Children[0])).Fill = brush;
            ((Path)(LowCanvas.Children[0])).Fill = brush;
            ((Path)(MutedCanvas.Children[0])).Fill = brush;

            if (Math.Abs(valuePercent) < 0.01)
            {
                return MutedCanvas;
            }
            if (valuePercent > 0 && valuePercent < 35)
            {
                return LowCanvas;
            }
            if (valuePercent >= 35 && valuePercent < 70)
            {
                return MediumCanvas;
            }
            if (valuePercent >= 70)
            {
                return HighCanvas;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
