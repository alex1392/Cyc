using System;

namespace Cyc
{
  /// <summary>
  /// 將<see cref="Action"/>封裝成命令的類別。
  /// </summary>
  public class RelayCommand : RelayCommandBase
  {
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    /// <summary>
    /// 將方法<paramref name="execute"/>封裝成命令，並使此命令永遠可執行。
    /// </summary>
    /// <param name="execute">要封裝成命令的方法。</param>
    public RelayCommand(Action execute)
    {
      _execute = execute;
      _canExecute = () => true;
    }
    /// <summary>
    /// 將方法<paramref name="execute"/>封裝成命令，並由<paramref name="canExecute"/>判斷此命令是否可執行。
    /// </summary>
    /// <param name="execute">要封裝成命令的方法。</param>
    /// <param name="canExecute">判斷<paramref name="action"/>是否可執行的方法。</param>
    public RelayCommand(Action execute, Func<bool> canExecute) : this(execute)
    {
      _canExecute = canExecute;
    }

    /// <summary>
    /// 判斷<see cref="Execute(object)"/>是否可以執行。
    /// </summary>
    public override bool CanExecute(object parameter = null) => _canExecute is null || _canExecute();

    /// <summary>
    /// 執行封裝的方法。
    /// </summary>
    public override void Execute(object parameter = null) => _execute();
  }

}
