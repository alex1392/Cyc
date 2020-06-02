using System;
using System.ComponentModel;

namespace Cyc
{
  /// <summary>
  /// 提供實現了<see cref="INotifyPropertyChanged"/>的基底類別。
  /// </summary>
  /// <remarks>請將ViewModel繼承自此類別，並使用Fody.PropertyChanged以連結View與ViewModel</remarks>
  public abstract class ObservableObject : INotifyPropertyChanged
  {
    /// <summary>
    /// 當其他屬性變更時觸發此事件。
    /// </summary>
    /// <remarks>請安裝Fody.PropertyChanged套件以自動呼叫此事件。</remarks>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// 提供方法呼叫<see cref="PropertyChanged"/>事件。
    /// </summary>
    /// <param name="property">發生<see cref="PropertyChanged"/>的屬性。</param>
    public void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    /// <summary>
    /// Fired when static property changed.
    /// </summary>
    public static event EventHandler<StaticPropertyChangedEventArgs> StaticPropertyChanged;
    public static void OnStaticPropertyChanged(string className, string propertyName)
    {
      StaticPropertyChanged?.Invoke(null, new StaticPropertyChangedEventArgs(className, propertyName));
    }
  }

  
}
