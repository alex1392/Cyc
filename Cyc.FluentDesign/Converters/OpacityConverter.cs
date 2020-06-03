using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cyc.FluentDesign.Converters {
	public class OpacityConverter : MarkupExtension, IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue) {
				return 0;
			}

			var opacity = double.Parse(values[0].ToString());
			var isEnter = (bool)values[1];
			return isEnter ? opacity : 0;
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		private static readonly OpacityConverter instance = new OpacityConverter();
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return instance;
		}
	}
}
