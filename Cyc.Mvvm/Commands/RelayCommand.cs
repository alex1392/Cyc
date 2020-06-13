using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cyc.Mvvm.Commands {
	/// <summary>
	/// A command which can always be executed.
	/// </summary>
	public class RelayCommand : ICommand {
		private readonly Action action;

		public event EventHandler CanExecuteChanged;

		public RelayCommand(Action action)
		{
			this.action = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			action?.Invoke();
		}
	}
}
