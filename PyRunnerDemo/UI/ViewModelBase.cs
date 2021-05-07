// ************************************************************************************************
// <copyright file="ViewModelBase.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace PyRunnerDemo.UI
{
	/// <summary>
	/// Implements <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}