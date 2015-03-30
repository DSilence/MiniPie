using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using MiniPie.Properties;

namespace MiniPie.Converter
{
    public class EnumToLocalizedStringConverter: IValueConverter
    {
        private const string FormatString = "Enum_{0}_{1}";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var typedValue = value as Type;
                if (targetType == typeof(IEnumerable) && typedValue != null && typedValue.IsEnum)
                {
                    var result = new List<string>();
                    foreach (var name in Enum.GetNames(typedValue))
                    {
                        var key = string.Format(FormatString, typedValue.Name, name);
                        var resultResource = Resources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
                        result.Add(resultResource);
                    }
                    return result;
                }
                if (value is Enum)
                {
                    var resultStringValue = value.ToString();
                    var key = string.Format(FormatString, value.GetType().Name, resultStringValue);
                    var resultResource = Resources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
                    if (resultResource != null)
                    {
                        return resultResource;
                    }
                    else
                    {
                        return resultStringValue;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (targetType.IsEnum)
                {
                    var enumNames = Enum.GetNames(targetType);
                    string stringValue = value.ToString();
                    foreach (var enumName in enumNames)
                    {
                        var key = string.Format(FormatString, targetType.Name, 
                            enumName);
                        var resultResource = Resources.ResourceManager
                            .GetString(key, CultureInfo.CurrentUICulture);
                        if (resultResource == stringValue)
                        {
                            return Enum.Parse(targetType, enumName);
                        }
                    }
                }
            }
            return null;
        }
    }
}
