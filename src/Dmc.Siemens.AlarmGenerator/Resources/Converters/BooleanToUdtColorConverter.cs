using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dmc.Siemens.AlarmGenerator.Resources
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public sealed class BooleanToUdtColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((bool)value) ? Brushes.Black : Brushes.Red;
            }
            catch
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush)
            {
                return (value as Brush).Equals(Brushes.Black);
            }
            return null;
        }

    }
}
