using Cyc.FluentDesign.Converters;
using SourceChord.FluentWPF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Forms = System.Windows.Forms;

namespace Cyc.FluentDesign {

	public static class WndIDs {

		#region Public Fields

		public const int WM_MOUSEHWHEEL = 0x020E;
		public const int WM_MOUSEWHEEL = 0x020A;

		#endregion Public Fields

		#region Public Methods

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

		#endregion Public Methods
	}

	public class MouseTiltEventArgs : EventArgs {

		#region Public Properties

		public int Tilt { get; set; }

		#endregion Public Properties

		#region Public Constructors

		public MouseTiltEventArgs(int tilt)
		{
			Tilt = tilt;
		}

		#endregion Public Constructors
	}

	public partial class RevealWindow : AcrylicWindow, INotifyPropertyChanged {

		#region Private Fields

		private StackPanel TitlebarControlsStackPanel;

		#endregion Private Fields

		#region Public Events

		public event EventHandler<MouseTiltEventArgs> MouseHWheel;

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Public Events

		#region Public Properties
		[Browsable(true)]
		[Description("Enable draging the window from anywhere inside the window.")]
		public bool EnableDragMove { get; set; } = false;

		[Browsable(true)]
		[Description("Show NotifyIcon Button in the title bar.")]
		public bool EnableNotifyIconButton { get; set; } = true;

		[Browsable(true)]
		[Description("Show Topmost Button in title bar.")]
		public bool EnableTopMostButton { get; set; } = true;

		[Browsable(true)]
		[Description("Add custom controls to title bar.")]
		public Collection<Control> TitlebarControls { get; set; } = new Collection<Control>();
		public ICommand NotifyIconCommand { get; set; }

		#endregion Public Properties

		#region Protected Properties

		protected Forms::NotifyIcon NotifyIcon { get; set; }

		#endregion Protected Properties

		#region Public Constructors

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

		#endregion Public Constructors

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			AddCustomWindowControls();
		}

		#endregion Public Methods

		#region Protected Methods

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			if (SizeToContent != SizeToContent.Manual) {
				InvalidateMeasure();
			}
		}

		/// <summary>
		/// Invoked by <see cref="HookMouseHWheel(int)"/>
		/// </summary>
		protected virtual void OnMouseHWheel(int delta)
		{
			MouseHWheel?.Invoke(this, new MouseTiltEventArgs(delta));
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			ProcessDragMove(e);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			HookWin32Message();
		}

		#endregion Protected Methods

		#region Private Methods

		private void AddCustomWindowControls()
		{
			TitlebarControlsStackPanel = GetTemplateChild(nameof(TitlebarControlsStackPanel)) as StackPanel;
			foreach (var control in TitlebarControls) {
				TitlebarControlsStackPanel.Children.Add(control);
			}
		}

		private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode != ResizeMode.NoResize;
		}

		private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Invoked by Win32 message.
		/// </summary>
		private void HookMouseHWheel(int delta)
		{
			OnMouseHWheel(delta); // propagate event
		}

		private void HookMouseWheel(int delta)
		{
			// Write behavior for MouseWheel event
		}

		private void HookWin32Message()
		{
			var source = PresentationSource.FromVisual(this) as HwndSource;
			source?.AddHook(WndProc); // Hook Win32 Messages to method
		}

		private void InitializeNotifyIcon()
		{
			NotifyIcon = new Forms::NotifyIcon();
			NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
			NotifyIconCommand = new MinimizeToIconCommand(MinimizeToNotifyIcon);
		}

		private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}

		private void MinimizeToNotifyIcon()
		{
			this.Hide();
			NotifyIcon.Visible = true;
		}

		private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void NotifyIcon_MouseDoubleClick(object sender, Forms::MouseEventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void ProcessDragMove(MouseButtonEventArgs e)
		{
			if (EnableDragMove && e.ButtonState == MouseButtonState.Pressed) {
				DragMove();
			}
		}

		private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}

		private void SetNotifyIcon()
		{
			NotifyIcon.Icon = (Icon as BitmapSource).ToBitmap().ToIcon();
		}

		private void SetSystemCommandBinding()
		{
			CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SetNotifyIcon();
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

		#endregion Private Methods
	}

	internal class MinimizeToIconCommand : ICommand {

		#region Private Fields

		private readonly Action action;

		#endregion Private Fields

		#region Public Events

		public event EventHandler CanExecuteChanged;

		#endregion Public Events

		#region Public Constructors

		public MinimizeToIconCommand(Action action)
		{
			this.action = action;
		}

		#endregion Public Constructors

		#region Public Methods

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			action?.Invoke();
		}

		#endregion Public Methods
	}
}