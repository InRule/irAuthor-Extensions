using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Controls
{
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return new GridLength((double)value, GridUnitType.Pixel);
            }

            if (value == null)
            {
                return new GridLength(0);
            }
            
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}