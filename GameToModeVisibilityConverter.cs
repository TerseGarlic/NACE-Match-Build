using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NACE_Match_Builder.Models;

namespace NACE_Match_Builder.Converters;

public class GameToModeVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int tabIndex)
        {
            if (parameter == null)
            {
                // For COD content (default with no parameter)
                return tabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // For Valorant content (with parameter)
                return tabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
