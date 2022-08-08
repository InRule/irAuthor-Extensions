using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics.Controls
{
    public class BooleanToVisibilityConverter : IValueConverter
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
                trueValue = Visibility.Visible;
            }

            if (trueValue == Visibility.Visible)
            {
                falseValue = Visibility.Collapsed;
            }
            else
            {
                falseValue = Visibility.Visible;
            }

            return value ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}