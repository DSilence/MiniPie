using System;
using System.Globalization;
using System.Windows.Data;
using MiniPie.Controls.CircularProgressButton;

namespace MiniPie.Converter.CircularProgressButton
{
    public class ProgressToAngleConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = System.Convert.ToDouble(values[0]);
            PropgressButton bar = values[1] as PropgressButton;

            if (bar != null)
            {
                return 359.999 * (progress / (bar.Maximum - bar.Minimum));
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}