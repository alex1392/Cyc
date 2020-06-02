using System;

namespace Cyc
{
  public class StaticPropertyChangedEventArgs : EventArgs
  {
    public StaticPropertyChangedEventArgs(string className, string propertyName)
    {
      ClassName = className;
      PropertyName = propertyName;
    }

    public string PropertyName { get; set; }
    public string ClassName { get; set; }
  }
}
