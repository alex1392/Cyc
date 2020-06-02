using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyc.Resource
{
  public class CycResources
  {
    public CycResources()
    {
      InitializeWindowButtonGeometryStyle();
    }

    /// <summary>
    /// Usage: <see cref="PackUri"/> + "AssemblyName;component/fileName"
    /// </summary>
    public static string PackUri { get; } 
      = @"pack://application:,,,/";

    /// <summary>
    /// Usage: <see cref="CurrentPackUri"/> + "filePath/fileName"
    /// </summary>
    public static string CurrentPackUri =>
      PackUri + Assembly.GetEntryAssembly().GetName().Name + @";component/";

    public static string CycResourcesPackUri { get; } 
      = PackUri + Assembly.GetExecutingAssembly().GetName().Name + @";component/";

    public static Uri PanCursorUri { get; } 
      = new Uri(CycResourcesPackUri + "pan.cur");
    public static Uri MouseLeftButtonUri { get; } 
      = new Uri(CycResourcesPackUri + "MouseLeftButton.png");
    public static Uri MouseRightButtonUri { get; } 
      = new Uri(CycResourcesPackUri + "MouseRightButton.png");
    public static Uri MouseWheelUri { get; } 
      = new Uri(CycResourcesPackUri + "MouseWheel.png");
    public static Uri CycIconUri { get; }
      = new Uri(CycResourcesPackUri + "icon_cyc.png");

    public static string CycIconUriString { get; }
      = CycIconUri.ToString();

    public static DrawingBrush CrossboardBrush { get; } = new DrawingBrush
    {
      TileMode = TileMode.Tile,
      Viewport = new Rect(0, 0, 32, 32),
      ViewportUnits = BrushMappingMode.Absolute,
      Drawing = new GeometryDrawing
      {
        Brush = Brushes.LightGray,
        Geometry = Geometry.Parse("M0,0 H16 V16 H32 V32 H16 V16 H0Z"),
      }
    };

    private static Style WindowButtonGeometryStyle;
    private void InitializeWindowButtonGeometryStyle()
    {
      WindowButtonGeometryStyle = new Style(typeof(Path));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.WidthProperty, 12));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.HeightProperty, 12));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.UseLayoutRoundingProperty, true));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.VerticalAlignmentProperty, VerticalAlignment.Center));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center));
      WindowButtonGeometryStyle.Setters.Add(new Setter(Path.FillProperty, new SolidColorBrush(Colors.White)));
    }

    public static Geometry CloseButtonGeometryData { get; } 
      = Geometry.Parse("M1,0 L6,5 L11,0 L12,1 L7,6 L12,11 L11,12 L6,7 L1,12 L0,11 L5,6 L0,1 z");
    public static Geometry RestoreButtonGeometryData { get; } 
      = Geometry.Parse("M1,3 L1,11 L9,11 L9,3 z M3,1 L3,2 L10,2 L10,9 L11,9 L11,1 z M2 ,0 L12,0 L12,10 L10,10 L10,12 L0,12 L0,2 L2 ,2 z");
    public static Geometry MaximizeButtonGeometryData { get; } 
      = Geometry.Parse("M1,1  L1 ,11 L11,11 L11,1 z M0,0 L12,0 L12,12 L0,12 z");
    public static Geometry MinimizeButtonGeometryData { get; } 
      = Geometry.Parse("M0,5 L12,5 L12,6 L0,6 z");
    public static Geometry TopmostButtonGeometryData { get; } 
      = Geometry.Parse("M 6 12 L 6 2 L 10 6 M 6 12 L 6 2 L 2 6 M 2 1 L 10 1");
    public static Geometry NotifyIconButtonGeometryData { get; } 
      = Geometry.Parse("M 6 12 L 6 3 L 10 6 M 6 12 L 6 3 L 2 6 M 2 .5 L 10 .5 M 2 2 L 10 2");

    public static SolidColorBrush ShadowBrush { get; } 
      = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.3), 0, 0, 0));
    public static SolidColorBrush DarkShadowBrush { get; } 
      = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.7), 0, 0, 0));

    public const string en_US = "en-US";
    public const string zh_TW = "zh-TW";
  }
}
