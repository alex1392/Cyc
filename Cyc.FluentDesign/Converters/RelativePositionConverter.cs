using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cyc.FluentDesign.Converters {

	public class RelativePositionConverter : MarkupExtension, IMultiValueConverter {

		#region Private Fields

		private static readonly RelativePositionConverter instance = new RelativePositionConverter();

		#endregion Private Fields

		#region Public Methods

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Any(o => o == DependencyProperty.UnsetValue || o == null)) {
				return new Point(0, 0);
			}

			var parent = values[0] as UIElement;
			var ctrl = values[1] as UIElement;
			var pointerPos = (Point)values[2];
			var relativePos = parent.TranslatePoint(pointerPos, ctrl);

			return relativePos;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return instance;
		}

		#endregion Public Methods
	}
}