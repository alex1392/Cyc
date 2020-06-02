using System;
using System.Globalization;
using System.Windows;

namespace Cyc
{
  public class VisibilityConverter : ValueConverterBase<VisibilityConverter>
  {
    /// <summary>
    /// Convert <see cref="bool"/> to <see cref="Visibility"/>
    /// </summary>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (Visibility)value == Visibility.Visible ? true : false;
    }
  }
}
