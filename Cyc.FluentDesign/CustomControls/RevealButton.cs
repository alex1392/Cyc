using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyc.FluentDesign
{
  public class RevealButton : Button
  {
    static RevealButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealButton), new FrameworkPropertyMetadata(typeof(RevealButton)));
    }

  }
}
