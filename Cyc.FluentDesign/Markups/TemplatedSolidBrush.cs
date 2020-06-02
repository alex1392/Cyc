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
  public class TemplatedSolidBrush : MarkupExtension
  {
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      var brush = new SolidColorBrush();
      var colorBinding = new Binding
      {
        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
        Path = new PropertyPath(PointerTracker.TrackerColorProperty),
      };
      var opacityBinding = new Binding
      {
        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
        Path = new PropertyPath(PointerTracker.TrackerOpacityProperty),
      };
      BindingOperations.SetBinding(brush, SolidColorBrush.ColorProperty, colorBinding);
      BindingOperations.SetBinding(brush, SolidColorBrush.OpacityProperty, opacityBinding);

      return brush;
    }
  }
}
