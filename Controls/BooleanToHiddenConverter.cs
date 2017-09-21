using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Controls
{
    public class BooleanToHiddenConverter : IValueConverter
    {
        public virtual object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            bool value;

            if (o is bool? || o is bool)
            {
                value = (bool)o;
            }
            else
            {
                throw new NotSupportedException("Only bool and nullable bool are supported.");
            }

            Visibility trueValue, falseValue;

            if (!(parameter is string && Enum.TryParse((string)parameter, out trueValue)))
            {
                trueValue = Visibility.Collapsed;
            }

            if (trueValue == Visibility.Collapsed)
            {
                falseValue = Visibility.Visible;
            }
            else
            {
                falseValue = Visibility.Collapsed;
            }

            return value ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}