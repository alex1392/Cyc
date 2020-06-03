using Cyc.FluentDesign.Converters;

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyc.FluentDesign.Brushes {

	public class RevealBrush : MarkupExtension {

		#region Public Methods

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var brush = new RadialGradientBrush
			{
				MappingMode = BrushMappingMode.Absolute
			};

			var sizeBinding = new Binding
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.TrackerSizeProperty),
			};
			BindingOperations.SetBinding(brush, RadialGradientBrush.RadiusXProperty, sizeBinding);
			BindingOperations.SetBinding(brush, RadialGradientBrush.RadiusYProperty, sizeBinding);

			var opacityBinding = new MultiBinding
			{
				Converter = new OpacityConverter()
			};
			opacityBinding.Bindings.Add(new Binding
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.TrackerOpacityProperty),
			});
			opacityBinding.Bindings.Add(new Binding
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.IsEnterProperty),
			});
			BindingOperations.SetBinding(brush, Brush.OpacityProperty, opacityBinding);

			var positionBinding = new MultiBinding
			{
				Converter = new RelativePositionConverter()
			};
			positionBinding.Bindings.Add(new Binding()
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.RootObjectProperty)
			});
			positionBinding.Bindings.Add(new Binding()
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
			});
			positionBinding.Bindings.Add(new Binding()
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.PositionProperty)
			});
			BindingOperations.SetBinding(brush, RadialGradientBrush.CenterProperty, positionBinding);
			BindingOperations.SetBinding(brush, RadialGradientBrush.GradientOriginProperty, positionBinding);

			var gradientStop1 = new GradientStop { Offset = 1, Color = Colors.Transparent };
			var gradientStop0 = new GradientStop { Offset = 0 };
			var gradientBinding = new Binding
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
				Path = new PropertyPath(PointerTracker.TrackerColorProperty),
			};
			BindingOperations.SetBinding(gradientStop0, GradientStop.ColorProperty, gradientBinding);
			brush.GradientStops.Add(gradientStop0);
			brush.GradientStops.Add(gradientStop1);

			return brush;
		}

		#endregion Public Methods
	}
}