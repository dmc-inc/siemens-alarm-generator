using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dmc.Siemens.AlarmGenerator.Resources
{
    public class DataBlockNumberValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is int && (int)value >= 0)
            {
                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Needs to be a positive integer");

        }
    }
}
