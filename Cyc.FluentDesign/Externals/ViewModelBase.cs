namespace Cyc
{
  /// <summary>
  /// 提供ViewMoel的基底功能。
  /// </summary>
  public class ViewModelBase : ObservableObject, IViewValidation
  {
    public bool IsViewValid { get; set; }
  }
}
