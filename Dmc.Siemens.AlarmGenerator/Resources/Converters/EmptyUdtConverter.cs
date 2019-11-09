using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Dmc.Siemens.AlarmGenerator.Resources
{
    [ValueConversion(typeof(object), typeof(KeyValuePair<string,bool>))]
    public sealed class EmptyUdtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is KeyValuePair<string, bool> && value != null)
            {
                return value;
            }
            else
            {
                return default(KeyValuePair<string, bool>);
            }

        }

    }
}
