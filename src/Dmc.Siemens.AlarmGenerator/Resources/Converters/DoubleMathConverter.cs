using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dmc.Siemens.AlarmGenerator.Resources
{
    [ValueConversion(typeof(double[]), typeof(double))]
    public class DoubleMathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values.All(v => v is double))
            {
                var runningTotal = (double)values[0];
                Func<double, double> mathOperation = null;
                if (parameter is string && !string.IsNullOrWhiteSpace(parameter as string))
                {
                    switch ((parameter as string).ToLower())
                    {
                        case "add":
                        case "+":
                        default:
                            mathOperation = d => runningTotal + d;
                            break;
                        case "sub":
                        case "subtract":
                        case "-":
                            mathOperation = d => runningTotal - d;
                            break;
                        case "mul":
                        case "multiply":
                        case "*":
                            mathOperation = d => runningTotal * d;
                            break;
                        case "div":
                        case "divide":
                        case "/":
                            mathOperation = d => (d != 0) ? runningTotal / d : 0.0;
                            break;
                    }
                }
                else
                {
                    mathOperation = d => runningTotal + d;
                }
                if (mathOperation != null)
                {
                    for (var i = 1; i < values.Length; i++)
                    {
                        runningTotal = mathOperation((double)values[i]);
                    }
                }

                return runningTotal;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
