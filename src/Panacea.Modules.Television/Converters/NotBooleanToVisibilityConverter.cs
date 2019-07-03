using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Panacea.Modules.Television.Converters
{
    class NotBooleanToVisibilityConverter : IValueConverter
    {
        BooleanToVisibilityConverter _conv = new BooleanToVisibilityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _conv.Convert(!(bool)value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
