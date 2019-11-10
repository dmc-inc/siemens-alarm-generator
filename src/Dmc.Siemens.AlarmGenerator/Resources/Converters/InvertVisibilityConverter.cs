using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dmc.Siemens.AlarmGenerator.Resources
{
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public sealed class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Visibility)
            {
                return ((Visibility)value == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();

        }

    }
}
