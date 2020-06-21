
using System;
using System.Windows.Input;

namespace Cyc.FluentDesign
{
	internal class RelayCommand : ICommand {

		#region Private Fields

		private readonly Action action;
		private readonly Func<bool> canAction;

		#endregion Private Fields

		#region Public Events

		public event EventHandler CanExecuteChanged;

		#endregion Public Events

		#region Public Constructors

		public RelayCommand(Action action, Func<bool> canAction = null)
		{
			this.action = action;
			this.canAction = canAction;
		}

		#endregion Public Constructors

		#region Public Methods

		public bool CanExecute(object parameter)
		{
			return canAction == null || canAction.Invoke();
		}

		public void Execute(object parameter)
		{
			action.Invoke();
		}

		#endregion Public Methods
	}
}