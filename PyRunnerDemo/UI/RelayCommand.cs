// ************************************************************************************************
// <copyright file="RelayCommand.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Windows.Input;

namespace PyRunnerDemo.UI
{
	/// <summary>
	/// A command that relays its functionality to other objects by invoking delegates.
	/// </summary>
	/// <remarks>
	/// Used for command binding in WPF.
	/// </remarks>
	/// <seealso cref="ICommand" />
	public class RelayCommand : ICommand
	{
		#region Fields

		private readonly Predicate<object> _canExecute;
		private readonly Action<object> _execute;

		#endregion // Fields

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		/// <exception cref="System.ArgumentNullException">
		/// Argument <paramref name="execute"/> is <c>null</c>.
		/// </exception>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion // Construction

		#region Operations (ICommand)

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (_canExecute != null)
				{
					CommandManager.RequerySuggested += value;
				}
			}
			remove
			{
				if (_canExecute != null)
				{
					CommandManager.RequerySuggested -= value;
				}
			}
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command.  If the command does not require data to be passed, this object can be set to null.
		/// </param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke(parameter) ?? true;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command.  If the command does not require data to be passed, this object can be set to null.
		/// </param>
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		#endregion // Operations (ICommand)
	}
}