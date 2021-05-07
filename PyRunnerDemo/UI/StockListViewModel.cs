// ************************************************************************************************
// <copyright file="StockListViewModel.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using PyRunnerDemo.Persistence;

namespace PyRunnerDemo.UI
{
	public class StockListViewModel : ViewModelBase
	{
		#region Fields

		private readonly MainViewModel _mainVm;

		private ObservableCollection<StockItem> _stocks;
		private ListCollectionView _collectionView;
		private string _tickerFilterText;
		private string _nameFilterText;
		private bool? _selectedFilter;

		private ICommand _uncheckAllCommand;
		private ICommand _checkRandomSampleCommand;
		private ICommand _checkAllVisibleItemsCommand;

		#endregion //  Fields

		#region Construction

		public StockListViewModel(MainViewModel mainViewModel)
		{
			_mainVm = mainViewModel;
			LoadStocks();
		}

		#endregion //  Construction

		#region Properties

		#region Commands

		public ICommand UncheckAllCommand =>
			_uncheckAllCommand ?? (_uncheckAllCommand = new RelayCommand(UncheckAll, (o) => _stocks.Any(si => si.IsSelected)));

		public ICommand CheckRandomSampleCommand =>
			_checkRandomSampleCommand ?? (_checkRandomSampleCommand = new RelayCommand(CheckRandomSample));

		public ICommand CheckAllVisibleItemsCommand =>
			_checkAllVisibleItemsCommand ?? (_checkAllVisibleItemsCommand = new RelayCommand(CheckAllVisibleItems, (o) => _collectionView.Count > 0));

		#endregion //  Commands

		public byte MinStocks { get; } = 6;
		public byte DefaultStocks { get; } = 18;
		public byte MaxStocks { get; } = 30;

		public ListCollectionView CollectionView
		{
			get => _collectionView;
			set
			{
				_collectionView = value;
				OnPropertyChanged();
			}
		}

		public string TickerFilterText
		{
			get => _tickerFilterText;
			set
			{
				if (_tickerFilterText != value)
				{
					_tickerFilterText = value;
					OnPropertyChanged();
					ApplyFilter();
				}
			}
		}

		public string NameFilterText
		{
			get => _nameFilterText;
			set
			{
				if (_nameFilterText != value)
				{
					_nameFilterText = value;
					OnPropertyChanged();
					ApplyFilter();
				}
			}
		}

		public bool? SelectedFilter
		{
			get => _selectedFilter;
			set
			{
				if (_selectedFilter != value)
				{
					_selectedFilter = value;
					OnPropertyChanged();
					ApplyFilter();
				}
			}
		}

		public ObservableCollection<StockItem> Stocks => _stocks;

		#endregion //  Properties

		#region Implementation

		private void LoadStocks()
		{
			var ctx = new SQLiteDatabaseContext(_mainVm.DbPath);

			var itemList = ctx.Stocks.ToList().Select(s => new StockItem(s)).ToList();
			_stocks = new ObservableCollection<StockItem>(itemList);
			_collectionView = new ListCollectionView(_stocks);

			// Initially sort the list by stock names
			ICollectionView view = CollectionViewSource.GetDefaultView(_collectionView);
			view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}

		private void ApplyFilter()
		{
			// ReSharper disable PossibleNullReferenceException
			_collectionView.Filter = (si) =>
			{
				var item = si as StockItem;

				bool hasTickerFilter = !string.IsNullOrWhiteSpace(_tickerFilterText);
				bool hasNameFilter = !string.IsNullOrWhiteSpace(_nameFilterText);

				bool result = true;

				if (hasTickerFilter)
				{
					string ticker = item.Ticker.ToLowerInvariant();
					string tickerFilter = _tickerFilterText.ToLowerInvariant();

					result = ticker.StartsWith(tickerFilter);
				}

				if (result && hasNameFilter)
				{
					string name = item.Name.ToLowerInvariant();
					string nameFilter = _nameFilterText.ToLowerInvariant();

					result = name.StartsWith(nameFilter);
				}

				return _selectedFilter.HasValue
					? result && item.IsSelected == _selectedFilter
					: result;
			};
			// ReSharper restore PossibleNullReferenceException

			OnPropertyChanged(nameof(CollectionView));
		}

		#region Commands

		private void CheckAllVisibleItems(object param)
		{
			_stocks.ToList().ForEach(si => si.IsSelected = false);

			_collectionView.Cast<StockItem>()
				.Take(MaxStocks)
				.ToList()
				.ForEach(si => si.IsSelected = true);

			NameFilterText = string.Empty;
			TickerFilterText = string.Empty;
			SelectedFilter = true;
		}

		private void UncheckAll(object param)
		{
			_stocks.ToList().ForEach(si => si.IsSelected = false);

			ApplyFilter();
		}

		private void CheckRandomSample(object param)
		{
			_stocks.ToList().ForEach(si => si.IsSelected = false);

			var random = new Random();

			for (int i = 0; i < DefaultStocks; i++)
			{
				_stocks[random.Next(_stocks.Count)].IsSelected = true;
			}

			NameFilterText = string.Empty;
			TickerFilterText = string.Empty;
			SelectedFilter = true;
			ApplyFilter();
		}

		#endregion //  Commands

		#endregion //  Implementation
	}
}