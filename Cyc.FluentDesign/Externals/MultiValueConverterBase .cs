using System;
using System.Globalization;
using System.Windows.Data;

namespace Cyc
{
  /// <summary>
  /// 提供具有<see cref="IMultiValueConverter"/>功能的類別。
  /// </summary>
  /// <typeparam name="TMultiValueConverter">要實作的MultiValueConverter類別。</typeparam>
  public abstract class MultiValueConverterBase<TMultiValueConverter> : ConverterMarkup<TMultiValueConverter>, IMultiValueConverter 
    where TMultiValueConverter : class, new()
  {
    public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    public abstract object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture);
  }
}
