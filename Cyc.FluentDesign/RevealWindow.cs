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

namespace Cyc.FluentDesign
{
	public partial class RevealWindow : AcrylicWindow, INotifyPropertyChanged {

		#region Public Fields

		public static readonly DependencyProperty MaximizeCommandProperty =
			DependencyProperty.Register("MaximizeCommand", typeof(ICommand), typeof(RevealWindow), new PropertyMetadata());

		public static readonly DependencyProperty MinimizeCommandProperty =
			DependencyProperty.Register("MinimizeCommand", typeof(ICommand), typeof(RevealWindow), new PropertyMetadata());

		public static readonly DependencyProperty RestoreCommandProperty =
			DependencyProperty.Register("RestoreCommand", typeof(ICommand), typeof(RevealWindow), new PropertyMetadata());

		#endregion Public Fields

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

		public ICommand MaximizeCommand {
			get { return (ICommand)GetValue(MaximizeCommandProperty); }
			set { SetValue(MaximizeCommandProperty, value); }
		}

		public ICommand MinimizeCommand {
			get { return (ICommand)GetValue(MinimizeCommandProperty); }
			set { SetValue(MinimizeCommandProperty, value); }
		}

		public ICommand NotifyIconCommand { get; set; }

		public ICommand RestoreCommand {
			get { return (ICommand)GetValue(RestoreCommandProperty); }
			set { SetValue(RestoreCommandProperty, value); }
		}

		[Browsable(true)]
		[Description("Add custom controls to title bar.")]
		public Collection<Control> TitlebarControls { get; set; } = new Collection<Control>();

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

			void InitializeNotifyIcon()
			{
				NotifyIcon = new Forms::NotifyIcon();
				NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
				NotifyIconCommand = new RelayCommand(MinimizeToNotifyIcon);
			}

			void SetSystemCommandBinding()
			{
				MaximizeCommand = new RelayCommand(MaximizeWindow, CanResizeWindow);
				MinimizeCommand = new RelayCommand(MinimizeWindow, CanMinimizeWindow);
				RestoreCommand = new RelayCommand(RestoreWindow, CanResizeWindow);

				CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
				CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
				CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
				CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
				CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
			}
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

		protected virtual void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanMinimizeWindow();
		}

		protected virtual void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanResizeWindow();
		}

		protected virtual void CloseWindow(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		protected virtual void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			MaximizeWindow();
		}

		protected virtual void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			MinimizeWindow();
		}

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

		protected virtual void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
		{
			RestoreWindow();
		}

		protected virtual void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
		{
			if (!(e.OriginalSource is FrameworkElement element)) {
				return;
			}

			var point = WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
				: new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
			point = element.TransformToAncestor(this).Transform(point);
			SystemCommands.ShowSystemMenu(this, point);
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

		private bool CanMinimizeWindow()
		{
			return ResizeMode != ResizeMode.NoResize;
		}

		private bool CanResizeWindow()
		{
			return ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		/// <summary>
		/// Invoked by Win32 message.
		/// </summary>
		private void HookMouseHWheel(int delta)
		{
			OnMouseHWheel(delta); // propagate event
		}

		private void HookWin32Message()
		{
			var source = PresentationSource.FromVisual(this) as HwndSource;
			source?.AddHook(WndProc); // Hook Win32 Messages to method
		}

		

		private void MaximizeWindow()
		{
			SystemCommands.MaximizeWindow(this);
		}

		private void MinimizeToNotifyIcon()
		{
			this.Hide();
			NotifyIcon.Visible = true;
		}

		private void MinimizeWindow()
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

		private void RestoreWindow()
		{
			SystemCommands.RestoreWindow(this);
		}

		private void SetNotifyIcon()
		{
			NotifyIcon.Icon = (Icon as BitmapSource).ToBitmap().ToIcon();
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
			switch (msg) {
				case WndIDs.WM_MOUSEHWHEEL:
					int delta = (short)wParam.HIWORD();
					HookMouseHWheel(delta);
					return (IntPtr)1;
			}
			return IntPtr.Zero;
		}

		#endregion Private Methods
	}
}