using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyc.FluentDesign {

	public static class PointerTracker {

		#region Public Fields

		public static readonly DependencyProperty EnableBackgroundColorProperty = DependencyProperty.RegisterAttached(
			"EnableBackgroundColor",
			typeof(bool),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty EnableBorderLightProperty = DependencyProperty.RegisterAttached(
			"EnableBorderLight",
			typeof(bool),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(PointerTracker), new PropertyMetadata(false, OnEnabledChanged));

		public static readonly DependencyProperty EnableHoverLightProperty = DependencyProperty.RegisterAttached(
			"EnableHoverLight",
			typeof(bool),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty EnablePressLightProperty = DependencyProperty.RegisterAttached(
			"EnablePressLight",
			typeof(bool),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty IsEnterProperty =
			DependencyProperty.RegisterAttached("IsEnter", typeof(bool), typeof(PointerTracker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty PositionProperty =
			DependencyProperty.RegisterAttached("Position", typeof(Point), typeof(PointerTracker), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty RootObjectProperty =
			DependencyProperty.RegisterAttached("RootObject", typeof(UIElement), typeof(PointerTracker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty TrackerColorProperty = DependencyProperty.RegisterAttached(
			"TrackerColor",
			typeof(Color),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty TrackerOpacityProperty = DependencyProperty.RegisterAttached(
			"TrackerOpacity",
			typeof(double),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(0.3, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty TrackerSizeProperty = DependencyProperty.RegisterAttached(
			"TrackerSize",
			typeof(double),
			typeof(PointerTracker),
			new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty XProperty =
			DependencyProperty.RegisterAttached("X", typeof(double), typeof(PointerTracker), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.Inherits));

		public static readonly DependencyProperty YProperty =
			DependencyProperty.RegisterAttached("Y", typeof(double), typeof(PointerTracker), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.Inherits));

		#endregion Public Fields

		#region Public Methods

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetEnableBackgroundColor(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnableBackgroundColorProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetEnableBorderLight(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnableBorderLightProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnabledProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetEnableHoverLight(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnableHoverLightProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetEnablePressLight(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnablePressLightProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static bool GetIsEnter(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsEnterProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static Point GetPosition(DependencyObject obj)
		{
			return (Point)obj.GetValue(PositionProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static UIElement GetRootObject(DependencyObject obj)
		{
			return (UIElement)obj.GetValue(RootObjectProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static Color GetTrackerColor(DependencyObject obj)
		{
			return (Color)obj.GetValue(TrackerColorProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static double GetTrackerOpacity(DependencyObject obj)
		{
			return (double)obj.GetValue(TrackerOpacityProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static double GetTrackerSize(DependencyObject obj)
		{
			return (double)obj.GetValue(TrackerSizeProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static double GetX(DependencyObject obj)
		{
			return (double)obj.GetValue(XProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static double GetY(DependencyObject obj)
		{
			return (double)obj.GetValue(YProperty);
		}

		public static void SetEnableBackgroundColor(DependencyObject obj, bool value)
		{
			obj.SetValue(EnableBackgroundColorProperty, value);
		}

		public static void SetEnableBorderLight(DependencyObject obj, bool value)
		{
			obj.SetValue(EnableBorderLightProperty, value);
		}

		public static void SetEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(EnabledProperty, value);
		}

		public static void SetEnableHoverLight(DependencyObject obj, bool value)
		{
			obj.SetValue(EnableHoverLightProperty, value);
		}

		public static void SetEnablePressLight(DependencyObject obj, bool value)
		{
			obj.SetValue(EnablePressLightProperty, value);
		}

		public static void SetTrackerColor(DependencyObject obj, Color value)
		{
			obj.SetValue(TrackerColorProperty, value);
		}

		public static void SetTrackerOpacity(DependencyObject obj, double value)
		{
			obj.SetValue(TrackerOpacityProperty, value);
		}

		public static void SetTrackerSize(DependencyObject obj, double value)
		{
			obj.SetValue(TrackerSizeProperty, value);
		}

		#endregion Public Methods

		#region Private Methods

		private static void Ctrl_MouseEnter(object sender, MouseEventArgs e)
		{
			var ctrl = sender as UIElement;
			if (ctrl != null) {
				SetIsEnter(ctrl, true);
			}
		}

		private static void Ctrl_MouseLeave(object sender, MouseEventArgs e)
		{
			var ctrl = sender as UIElement;
			if (ctrl != null) {
				SetIsEnter(ctrl, false);
			}
		}

		private static void Ctrl_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			var ctrl = sender as UIElement;
			if (ctrl != null && GetIsEnter(ctrl)) {
				var pos = e.GetPosition(ctrl);

				SetX(ctrl, pos.X);
				SetY(ctrl, pos.Y);
				SetPosition(ctrl, pos);
			}
		}

		private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = d as UIElement;
			var newValue = (bool)e.NewValue;
			var oldValue = (bool)e.OldValue;
			if (ctrl == null) {
				return;
			}

			// 無効になった場合の処理
			if (oldValue && !newValue) {
				ctrl.MouseEnter -= Ctrl_MouseEnter;
				ctrl.PreviewMouseMove -= Ctrl_PreviewMouseMove;
				ctrl.MouseLeave -= Ctrl_MouseLeave;

				ctrl.ClearValue(PointerTracker.RootObjectProperty);
			}

			// 有効になった場合の処理
			if (!oldValue && newValue) {
				ctrl.MouseEnter += Ctrl_MouseEnter;
				ctrl.PreviewMouseMove += Ctrl_PreviewMouseMove;
				ctrl.MouseLeave += Ctrl_MouseLeave;

				SetRootObject(ctrl, ctrl);
			}
		}

		private static void SetIsEnter(DependencyObject obj, bool value)
		{
			obj.SetValue(IsEnterProperty, value);
		}

		private static void SetPosition(DependencyObject obj, Point value)
		{
			obj.SetValue(PositionProperty, value);
		}

		private static void SetRootObject(DependencyObject obj, UIElement value)
		{
			obj.SetValue(RootObjectProperty, value);
		}

		private static void SetX(DependencyObject obj, double value)
		{
			obj.SetValue(XProperty, value);
		}

		private static void SetY(DependencyObject obj, double value)
		{
			obj.SetValue(YProperty, value);
		}

		#endregion Private Methods
	}
}