using System;
using System.Windows.Input;

namespace EDIViewer.ViewModel.Common
{
	public class SimpleRelayCommand : ICommand
	{
		private readonly Action<object> execute;

		public event EventHandler CanExecuteChanged;

		public SimpleRelayCommand(Action<object> execute)
		{
			this.execute = execute;
		}

		public void Execute(object parameter)
		{
			this.execute(parameter);
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}
	}
}