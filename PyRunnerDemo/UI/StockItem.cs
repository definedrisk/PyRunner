// ************************************************************************************************
// <copyright file="StockItem.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using PyRunnerDemo.Core;

namespace PyRunnerDemo.UI
{
	/// <summary>
	/// Wrapper class for a <see cref="Stock"/> entity as needed for the UI.
	/// </summary>
	public class StockItem
	{
		private bool _isSelected;

		public StockItem(Stock stock)
		{
			if (stock == null)
			{
				throw new ArgumentNullException(nameof(stock));
			}

			Id = stock.Id;
			Ticker = stock.Ticker;
			Name = stock.Name;
		}

// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public long Id { get; }

		public string Ticker { get; }

		public string Name { get; }

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnSelectionChanged();
				}
			}
		}

		public event EventHandler SelectionChanged;

		protected void OnSelectionChanged()
		{
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}