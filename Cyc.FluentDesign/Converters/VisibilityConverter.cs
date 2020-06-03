using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cyc.FluentDesign.Converters {

	public class VisibilityConverter : MarkupExtension, IValueConverter {

		#region Private Fields

		private static readonly VisibilityConverter instance = new VisibilityConverter();

		#endregion Private Fields

		#region Public Methods

		/// <summary>
		/// Convert <see cref="bool"/> to <see cref="Visibility"/>
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Visible ? true : false;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return instance;
		}

		#endregion Public Methods
	}
}