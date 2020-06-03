using Cyc.FluentDesign.Utility;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Cyc.FluentDesign {

	public class AcrylicWindow : Window {
		static AcrylicWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicWindow), new FrameworkPropertyMetadata(typeof(AcrylicWindow)));

			TintColorProperty = AcrylicElement.TintColorProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));
			TintOpacityProperty = AcrylicElement.TintOpacityProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.Inherits));
			NoiseOpacityProperty = AcrylicElement.NoiseOpacityProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));
			FallbackColorProperty = AcrylicElement.FallbackColorProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(Colors.LightGray, FrameworkPropertyMetadataOptions.Inherits));
			ShowTitleBarProperty = AcrylicElement.ShowTitleBarProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
			ExtendViewIntoTitleBarProperty = AcrylicElement.ExtendViewIntoTitleBarProperty.AddOwner(typeof(AcrylicWindow), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			EnableBlur(this);
		}

		internal static void EnableBlur(Window win)
		{
			var windowHelper = new WindowInteropHelper(win);

			// ウィンドウに半透明のアクリル効果を適用する
			AcrylicHelper.EnableBlur(windowHelper.Handle);

			win.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, __) => { SystemCommands.CloseWindow(win); }));
			win.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, __) => { SystemCommands.MinimizeWindow(win); }));
			win.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, __) => { SystemCommands.MaximizeWindow(win); }));
			win.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, __) => { SystemCommands.RestoreWindow(win); }));

			// WPFのSizeToContentのバグ対策
			// (WindowChrome使用時に、SizeToContentのウィンドウサイズ計算が正しく行われない)
			void onContentRendered(object sender, EventArgs e)
			{
				if (win.SizeToContent != SizeToContent.Manual) {
					win.InvalidateMeasure();
				}

				win.ContentRendered -= onContentRendered;
			}
			win.ContentRendered += onContentRendered;
		}
		#region Dependency Property

		public Color TintColor {
			get { return (Color)GetValue(TintColorProperty); }
			set { SetValue(TintColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TintColorProperty;
		public static Color GetTintColor(DependencyObject obj)
		{
			return (Color)obj.GetValue(AcrylicElement.TintColorProperty);
		}

		public static void SetTintColor(DependencyObject obj, Color value)
		{
			obj.SetValue(AcrylicElement.TintColorProperty, value);
		}


		public double TintOpacity {
			get { return (double)GetValue(TintOpacityProperty); }
			set { SetValue(TintOpacityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TintOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TintOpacityProperty;
		public static double GetTintOpacity(DependencyObject obj)
		{
			return (double)obj.GetValue(AcrylicElement.TintOpacityProperty);
		}

		public static void SetTintOpacity(DependencyObject obj, double value)
		{
			obj.SetValue(AcrylicElement.TintOpacityProperty, value);
		}



		public double NoiseOpacity {
			get { return (double)GetValue(NoiseOpacityProperty); }
			set { SetValue(NoiseOpacityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for NoiseOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty NoiseOpacityProperty;
		public static double GetNoiseOpacity(DependencyObject obj)
		{
			return (double)obj.GetValue(AcrylicElement.NoiseOpacityProperty);
		}

		public static void SetNoiseOpacity(DependencyObject obj, double value)
		{
			obj.SetValue(AcrylicElement.NoiseOpacityProperty, value);
		}


		public Color FallbackColor {
			get { return (Color)GetValue(FallbackColorProperty); }
			set { SetValue(FallbackColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FallbackColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FallbackColorProperty;
		public static Color GetFallbackColor(DependencyObject obj)
		{
			return (Color)obj.GetValue(AcrylicElement.FallbackColorProperty);
		}

		public static void SetFallbackColor(DependencyObject obj, Color value)
		{
			obj.SetValue(AcrylicElement.FallbackColorProperty, value);
		}

		public bool ShowTitleBar {
			get { return (bool)GetValue(ShowTitleBarProperty); }
			set { SetValue(ShowTitleBarProperty, value); }
		}
		public static readonly DependencyProperty ShowTitleBarProperty;
		public static bool GetShowTitleBar(DependencyObject obj)
		{
			return (bool)obj.GetValue(AcrylicElement.ShowTitleBarProperty);
		}

		public static void SetShowTitleBar(DependencyObject obj, bool value)
		{
			obj.SetValue(AcrylicElement.ShowTitleBarProperty, value);
		}


		public bool ExtendViewIntoTitleBar {
			get { return (bool)GetValue(ExtendViewIntoTitleBarProperty); }
			set { SetValue(ExtendViewIntoTitleBarProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ExtendViewIntoTitleBar.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ExtendViewIntoTitleBarProperty;
		public static bool GetExtendViewIntoTitleBar(DependencyObject obj)
		{
			return (bool)obj.GetValue(AcrylicElement.ExtendViewIntoTitleBarProperty);
		}

		public static void SetExtendViewIntoTitleBar(DependencyObject obj, bool value)
		{
			obj.SetValue(AcrylicElement.ExtendViewIntoTitleBarProperty, value);
		}




		#endregion


		#region Attached Property


		public static bool GetEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnabledProperty);
		}

		public static void SetEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(EnabledProperty, value);
		}

		// Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(AcrylicWindow), new PropertyMetadata(false, OnEnableChanged));

		private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var win = d as Window;
			if (win == null) { return; }

			var value = (bool)e.NewValue;
			if (value) {
				var dic = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Cyc.FluentDesign;component/Styles/Window.xaml") };
				var style = dic["AcrylicWindowStyle"] as Style;
				win.Style = style;

				win.Loaded += (_, __) => { EnableBlur(win); };
				if (win.IsLoaded)
					EnableBlur(win);
			}
		}
		#endregion
	}

	internal class AcrylicElement {
		public static Color GetTintColor(DependencyObject obj)
		{
			return (Color)obj.GetValue(TintColorProperty);
		}

		public static void SetTintColor(DependencyObject obj, Color value)
		{
			obj.SetValue(TintColorProperty, value);
		}

		// Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TintColorProperty =
			DependencyProperty.RegisterAttached("TintColor", typeof(Color), typeof(AcrylicElement), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));




		public static double GetTintOpacity(DependencyObject obj)
		{
			return (double)obj.GetValue(TintOpacityProperty);
		}

		public static void SetTintOpacity(DependencyObject obj, double value)
		{
			obj.SetValue(TintOpacityProperty, value);
		}

		// Using a DependencyProperty as the backing store for TintOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TintOpacityProperty =
			DependencyProperty.RegisterAttached("TintOpacity", typeof(double), typeof(AcrylicElement), new PropertyMetadata(0.6));




		public static double GetNoiseOpacity(DependencyObject obj)
		{
			return (double)obj.GetValue(NoiseOpacityProperty);
		}

		public static void SetNoiseOpacity(DependencyObject obj, double value)
		{
			obj.SetValue(NoiseOpacityProperty, value);
		}

		// Using a DependencyProperty as the backing store for NoiseOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty NoiseOpacityProperty =
			DependencyProperty.RegisterAttached("NoiseOpacity", typeof(double), typeof(AcrylicElement), new PropertyMetadata(0.03));




		public static Color GetFallbackColor(DependencyObject obj)
		{
			return (Color)obj.GetValue(FallbackColorProperty);
		}

		public static void SetFallbackColor(DependencyObject obj, Color value)
		{
			obj.SetValue(FallbackColorProperty, value);
		}

		// Using a DependencyProperty as the backing store for FallbackColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FallbackColorProperty =
			DependencyProperty.RegisterAttached("FallbackColor", typeof(Color), typeof(AcrylicElement), new PropertyMetadata(Colors.LightGray));





		public static bool GetShowTitleBar(DependencyObject obj)
		{
			return (bool)obj.GetValue(ShowTitleBarProperty);
		}

		public static void SetShowTitleBar(DependencyObject obj, bool value)
		{
			obj.SetValue(ShowTitleBarProperty, value);
		}

		// Using a DependencyProperty as the backing store for ShowTitleBar.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShowTitleBarProperty =
			DependencyProperty.RegisterAttached("ShowTitleBar", typeof(bool), typeof(AcrylicElement), new PropertyMetadata(true));



		public static bool GetExtendViewIntoTitleBar(DependencyObject obj)
		{
			return (bool)obj.GetValue(ExtendViewIntoTitleBarProperty);
		}

		public static void SetExtendViewIntoTitleBar(DependencyObject obj, bool value)
		{
			obj.SetValue(ExtendViewIntoTitleBarProperty, value);
		}

		// Using a DependencyProperty as the backing store for ExtendViewIntoTitleBar.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ExtendViewIntoTitleBarProperty =
			DependencyProperty.RegisterAttached("ExtendViewIntoTitleBar", typeof(bool), typeof(AcrylicElement), new PropertyMetadata(false));


	}
}
