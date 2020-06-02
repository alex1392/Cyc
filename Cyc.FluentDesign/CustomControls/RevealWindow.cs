using System.Windows;

namespace Cyc.FluentDesign {
    public class RevealWindow : CustomWindowBase
  {
    static RevealWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealWindow), new FrameworkPropertyMetadata(typeof(RevealWindow)));
    }
  }
}
