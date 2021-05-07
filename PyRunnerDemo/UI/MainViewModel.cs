// ************************************************************************************************
// <copyright file="MainViewModel.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PyRunner;

namespace PyRunnerDemo.UI
{
	public class MainViewModel : ViewModelBase
	{
		#region Fields

		private bool _analyzing;

		private DateTime _startDate;
		private DateTime _endDate;
		private bool _needsChartRedraw;
		private bool _needsTreeRefresh;
		private byte _numClusters;

		private ICommand _doAnalysisCommand;

		#endregion //  Fields

// ReSharper disable AssignNullToNotNullAttribute
		public MainViewModel()
		{
			// Initialize some defaults.
			_startDate = MinStartDate;
			_endDate = MaxEndDate;
			_numClusters = DefaultClusters;
			_needsTreeRefresh = true;
			_needsChartRedraw = true;

			PythonRunner = new PythonRunner(ConfigurationManager.AppSettings["pythonPath"], 20000);
			DbPath = Path.GetFullPath(ConfigurationManager.AppSettings["dbFile"]);

			string chartScript = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				ConfigurationManager.AppSettings["scripts"],
				"chart.py");

			string kMeansScript = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				ConfigurationManager.AppSettings["scripts"],
				"kmeans.py");

			// Initialize 'child' VMs.
			StockListVm = new StockListViewModel(this);
			TreeViewVm = new TreeViewViewModel(this, kMeansScript);
			ChartVm = new ChartViewModel(this, chartScript);

			// 'Invalidate' chart and tree when stock selection changes.
			StockListVm.Stocks.ToList().ForEach(si => si.SelectionChanged += (sender, args) =>
			{
				NeedsChartRedraw = true;
				NeedsTreeRefresh = true;
				OnPropertyChanged(nameof(StockListVm.CollectionView));
			});
		}
// ReSharper restore AssignNullToNotNullAttribute

		#region Properties

		#region Commands

		public ICommand DoAnalysisCommand =>
			_doAnalysisCommand ?? (_doAnalysisCommand = new RelayCommand(DoAnalysis, CanDoAnalysis));

		#endregion //  Commands

		/// <summary>
		/// ViewModel containing data and logic for the list of stocks (left part of the window).
		/// </summary>
		public StockListViewModel StockListVm { get; }

		/// <summary>
		/// ViewModel containing data and logic for the line chart (middle part of the window).
		/// </summary>
		public ChartViewModel ChartVm { get; }

		/// <summary>
		/// ViewModel containing data and logic for the treeview with the results of the k-Means
		/// clustering analysis of the stock price movements (which is at the same time the color
		/// legend for the chart; middle part of the window).
		/// </summary>
		public TreeViewViewModel TreeViewVm { get; }

		/// <summary>
		/// Full path to the stocks database.
		/// </summary>
		internal string DbPath { get; }

		/// <summary>
		/// Python script runner.
		/// </summary>
		internal PythonRunner PythonRunner { get; }

		/// <summary>
		/// Selected stocks.
		/// </summary>
		internal List<StockItem> SelectedItems { get; private set; }

		/// <summary>
		/// Ticker symbols of the <see cref="SelectedItems"/>.
		/// </summary>
		internal string TickerList { get; private set; }

		public byte MinClusters { get; } = 3;
		public byte DefaultClusters { get; } = 6;
		public byte MaxClusters { get; } = 10;

		public DateTime MinStartDate { get; } = DateTime.Parse("2010-01-01");
		public DateTime MaxStartDate { get; } = DateTime.Parse("2014-01-01");
		public DateTime MinEndDate { get; } = DateTime.Parse("2015-12-01");
		public DateTime MaxEndDate { get; } = DateTime.Parse("2019-07-01");

		/// <summary>
		/// Number of clusters metaparameter for k-Means analysis.
		/// </summary>
		public byte NumClusters
		{
			get => _numClusters;
			set
			{
				if (_numClusters != value)
				{
					_numClusters = value;
					OnPropertyChanged();
					NeedsTreeRefresh = true;
				}
			}
		}

		/// <summary>
		/// Starting date for stock analysis data. Only year/month are relevant.
		/// </summary>
		public DateTime StartDate
		{
			get => _startDate;
			set
			{
				if (_startDate != value)
				{
					_startDate = value;
					OnPropertyChanged();
					NeedsChartRedraw = true;
					NeedsTreeRefresh = true;
				}
			}
		}

		/// <summary>
		/// End date for stock analysis data. Only year/month are relevant.
		/// </summary>
		public DateTime EndDate
		{
			get => _endDate;
			set
			{
				if (_endDate != value)
				{
					_endDate = value;
					OnPropertyChanged();
					NeedsChartRedraw = true;
					NeedsTreeRefresh = true;
				}
			}
		}

		/// <summary>
		/// Flag that indicates whether the chart is out of sync with the window's
		/// other controls. In such a case, it is not displayed until the next call
		/// to the <see cref="DoAnalysis"/> method.
		/// </summary>
		public bool NeedsChartRedraw
		{
			get => _needsChartRedraw;
			set
			{
				if (_needsChartRedraw != value)
				{
					_needsChartRedraw = value;
					OnPropertyChanged();
					if (_needsChartRedraw)
					{
						ChartVm.SummaryChartText = ChartViewModel.ChartNeedsRedrawing;
					}
				}
			}
		}

		/// <summary>
		/// Flag that indicates whether the treeview is out of sync with the window's
		/// other controls. In such a case, it is not displayed until the next call
		/// to the <see cref="DoAnalysis"/> method.
		/// </summary>
		public bool NeedsTreeRefresh
		{
			get => _needsTreeRefresh;
			set
			{
				if (_needsTreeRefresh != value)
				{
					_needsTreeRefresh = value;
					OnPropertyChanged();
					if (_needsTreeRefresh)
					{
						TreeViewVm.TreeViewText = TreeViewViewModel.TreeNeedsRefresh;
					}
				}
			}
		}

		#endregion //  Properties

		#region Implementation

		#region Commands

		/// <summary>
		/// Performs the various analysis steps in delegating the main functionality to the child viewmodels.
		/// </summary>
		/// <param name="param">[unused]</param>
		private async void DoAnalysis(object param)
		{
			try
			{
				_analyzing = true; // avoid overlapping calls

				SelectedItems = StockListVm.Stocks
					.Where(si => si.IsSelected)
					.OrderBy(si => si.Ticker)
					.ToList();
				TickerList = SelectedItems
					.Select(si => "'" + si.Ticker + "'")
					.OrderBy(s => s)
					.Aggregate((s1, s2) => s1 + "," + s2);

				if (NeedsChartRedraw)
				{
					NeedsChartRedraw = !await ChartVm.DrawChart();
				}

				if (NeedsTreeRefresh)
				{
					NeedsTreeRefresh = !await TreeViewVm.PerformKMeans();
				}
			}
			finally
			{
				_analyzing = false;
			}
		}

		private bool CanDoAnalysis(object obj) =>
			!_analyzing &&
			(NeedsChartRedraw || NeedsTreeRefresh) &&
			StockListVm.CollectionView.Count >= StockListVm.MinStocks && 
			StockListVm.CollectionView.Count <= StockListVm.MaxStocks;

		#endregion //  Commands

		#endregion //  Implementation
	}
}