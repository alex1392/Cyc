using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyc.FluentDesign
{
    //public class RevealBrushExtension : MarkupExtension {
    //    public RevealBrushExtension()
    //    {

    //    }

    //    public Color Color { get; set; } = Colors.Black;
    //    public double Opacity { get; set; } = 1;

    //    public double Size { get; set; } = 100;

    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
    //        var target = pvt.TargetObject as DependencyObject;

    //        // 円形のグラデーション表示をするブラシを作成
    //        var bgColor = Color.FromArgb(0, this.Color.R, this.Color.G, this.Color.B);
    //        var brush = new RadialGradientBrush(this.Color, bgColor);
    //        brush.MappingMode = BrushMappingMode.Absolute;
    //        brush.RadiusX = this.Size;
    //        brush.RadiusY = this.Size;

    //        // カーソルが領域外にある場合は、透明にする。
    //        var opacityBinding = new Binding("Opacity")
    //        {
    //            Source = target,
    //            Path = new PropertyPath(PointerTracker.IsEnterProperty),
    //            Converter = new OpacityConverter2(),
    //            ConverterParameter = this.Opacity
    //        };
    //        BindingOperations.SetBinding(brush, RadialGradientBrush.OpacityProperty, opacityBinding);

    //        // グラデーションの中心位置をバインディング
    //        var binding = new MultiBinding();
    //        binding.Converter = new RelativePositionConverter();
    //        binding.Bindings.Add(new Binding() { Source = target, Path = new PropertyPath(PointerTracker.RootObjectProperty) });
    //        binding.Bindings.Add(new Binding() { Source = target });
    //        binding.Bindings.Add(new Binding() { Source = target, Path = new PropertyPath(PointerTracker.PositionProperty) });

    //        BindingOperations.SetBinding(brush, RadialGradientBrush.CenterProperty, binding);
    //        BindingOperations.SetBinding(brush, RadialGradientBrush.GradientOriginProperty, binding);
    //        return brush;
    //    }
    //}
    public class RevealBrush : MarkupExtension
  {
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

      var opacityBinding = new MultiBinding();
      opacityBinding.Converter = new OpacityConverter();
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

      var positionBinding = new MultiBinding();
      positionBinding.Converter = new RelativePositionConverter();
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
  }
}
