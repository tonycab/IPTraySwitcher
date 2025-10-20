using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Linq;

namespace IPTraySwitcherWPF
{
    public class ValidationErrorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is System.Collections.IList errors && errors.Count > 0)
            {
                var firstError = errors[0] as ValidationError;
                return firstError?.ErrorContent?.ToString();
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

