using Cyc.FluentDesign.Commands;
using Cyc.FluentDesign.Converters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Cyc.FluentDesign {
	public class RevealWindow : Window, INotifyPropertyChanged {
		/// <summary>
		/// Allow usage in xaml
		/// </summary>
		static RevealWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealWindow), new FrameworkPropertyMetadata(typeof(RevealWindow)));
		}
		public RevealWindow()
		{
			SetSystemCommandBinding();
			InitializeNotifyIcon();
			Loaded += Window_Loaded;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SetNotifyIcon();
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			if (SizeToContent != SizeToContent.Manual) {
				InvalidateMeasure();
			}
		}
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			AddCustomWindowControls();
		}
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			HookWin32Message();
		}

		#region Win32 Messages
		private void HookWin32Message()
		{
			var source = PresentationSource.FromVisual(this) as HwndSource;
			source?.AddHook(WndProc); // Hook Win32 Messages to method
		}
		/// <summary>
		/// Processing Win32 messages.
		/// </summary>
		/// <param name="hwnd">Window handle</param>
		/// <param name="msg">Message ID</param>
		/// <param name="wParam">Message "w" pointer.</param>
		/// <param name="lParam">Message "l" pointer.</param>
		/// <returns>Return specific value for specific message. Check documents of Win32 message processing.</returns>
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			int delta;
			switch (msg) {
				case WndIDs.WM_MOUSEHWHEEL:
					delta = (short)wParam.HIWORD();
					HookMouseHWheel(delta);
					return (IntPtr)1;
				case WndIDs.WM_MOUSEWHEEL:
					delta = (short)wParam.HIWORD();
					HookMouseWheel(delta);
					return (IntPtr)1;
			}
			return IntPtr.Zero;
		}

		/// <summary>
		/// Invoked by Win32 message.
		/// </summary>
		private void HookMouseHWheel(int delta)
		{
			OnMouseHWheel(delta); // propagate event
		}
		public event EventHandler<MouseTiltEventArgs> MouseHWheel;
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked by <see cref="HookMouseHWheel(int)"/>
		/// </summary>
		protected virtual void OnMouseHWheel(int delta)
		{
			MouseHWheel?.Invoke(this, new MouseTiltEventArgs(delta));
		}

		private void HookMouseWheel(int delta)
		{
			// Write behavior for MouseWheel event
		}
		#endregion

		#region TitlebarControls
		public Collection<Control> TitlebarControls { get; set; } = new Collection<Control>();
		private StackPanel TitlebarControlsStackPanel;
		private void AddCustomWindowControls()
		{
			TitlebarControlsStackPanel = GetTemplateChild(nameof(TitlebarControlsStackPanel)) as StackPanel;
			foreach (var control in TitlebarControls) {
				TitlebarControlsStackPanel.Children.Add(control);
			}
		}
		#endregion

		#region NotifyIcon
		[Browsable(true)]

		[Description("Showing NotifyIcon Button in the title bar")]
		public bool EnableNotifyIconButton { get; set; } = true;
		protected Forms::NotifyIcon NotifyIcon { get; set; }
		public ICommand NotifyIconCommand { get; set; }

		private void InitializeNotifyIcon()
		{
			NotifyIcon = new Forms::NotifyIcon();
			NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
			NotifyIconCommand = new RelayCommand(MinimizeToNotifyIcon);
		}
		private void SetNotifyIcon()
		{
			NotifyIcon.Icon = (Icon as BitmapSource).ToBitmap().ToIcon();
		}
		private void MinimizeToNotifyIcon()
		{
			Hide();
			NotifyIcon.Visible = true;
		}
		private void NotifyIcon_MouseDoubleClick(object sender, Forms::MouseEventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}
		#endregion

		#region TopmostButton
		[Browsable(true)]

		[Description("Showing Topmost Button in title bar")]
		public bool EnableTopmostButton { get; set; } = true;
		#endregion

		#region SystemCommands
		private void SetSystemCommandBinding()
		{
			CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
		}
		private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
		}
		private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode != ResizeMode.NoResize;
		}
		private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}
		private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}
		private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}
		private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}
		private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
		{
			if (!(e.OriginalSource is FrameworkElement element)) {
				return;
			}

			var point = WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
				: new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
			point = element.TransformToAncestor(this).Transform(point);
			SystemCommands.ShowSystemMenu(this, point);
		}
		#endregion

		#region DragMove
		public bool EnableDragMove { get; set; } = false;
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			ProcessDragMove(e);
		}
		private void ProcessDragMove(MouseButtonEventArgs e)
		{
			if (EnableDragMove && e.ButtonState == MouseButtonState.Pressed) {
				DragMove();
			}
		}
		#endregion
	}

	public class MouseTiltEventArgs : EventArgs {
		public int Tilt { get; set; }
		public MouseTiltEventArgs(int tilt)
		{
			Tilt = tilt;
		}
	}

	public static class PointerExtensions {
		/// <summary>
		/// Gets high bits values of the pointer.
		/// </summary>
		public static int HIWORD(this IntPtr ptr)
		{
			return (ptr.ToInt32() >> 16) & 0xFFFF;
		}

		/// <summary>
		/// Gets low bits values of the pointer.
		/// </summary>
		public static int LOWORD(this IntPtr ptr)
		{
			return ptr.ToInt32() & 0xFFFF;
		}
	}

	public static class WndIDs {
		public const int WM_MOUSEHWHEEL = 0x020E;
		public const int WM_MOUSEWHEEL = 0x020A;
	}
}
