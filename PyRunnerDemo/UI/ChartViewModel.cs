// ************************************************************************************************
// <copyright file="ChartViewModel.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PyRunnerDemo.UI
{
	public class ChartViewModel : ViewModelBase
	{
		#region Constants

		public const string SelectSomeStocks = "Select some stocks.";
		public const string ChartNeedsRedrawing = "Redraw required.";
		public const string Processing = "Processing ...";

		#endregion //  Constants

		#region Fields

		private readonly MainViewModel _mainVm;

		private BitmapSource _summaryChart;
		private string _summaryChartText;

		#endregion //  Fields

		#region Construction

		/// <summary>
		/// Creates a new instance of the <see cref="ChartViewModel"/> class.
		/// </summary>
		/// <param name="mainViewModel">The 'parent' viewmodel.</param>
		/// <param name="script">The python script.</param>
		public ChartViewModel(MainViewModel mainViewModel, string script)
		{
			_mainVm = mainViewModel;
			DrawSummaryLineChartScript = script;

			SummaryChartText = SelectSomeStocks;
		}

		#endregion //  Construction

		#region Properties

		/// <summary>
		/// Full path to the script.
		/// </summary>
		internal string DrawSummaryLineChartScript { get; }

		/// <summary>
		/// The chart object.
		/// </summary>
		public BitmapSource SummaryChart
		{
			get => _summaryChart;
			set
			{
				if (!Equals(_summaryChart, value))
				{
					_summaryChart = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// A text that is displayed in some situations instead of the chart.
		/// </summary>
		public string SummaryChartText
		{
			get => _summaryChartText;
			set
			{
				if (_summaryChartText != value)
				{
					_summaryChartText = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion //  Properties

		#region Implementation

		/// <summary>
		/// Calls the python script to draw the chart of the selected stocks.
		/// </summary>
		/// <returns>True on success.</returns>
		internal async Task<bool> DrawChart()
		{
			SummaryChartText = Processing;
			SummaryChart = null;

			try
			{
				var bitmap = await _mainVm.PythonRunner.GetImageAsync(
					DrawSummaryLineChartScript,
					_mainVm.DbPath,
					_mainVm.TickerList,
					_mainVm.StartDate.ToString("yyyy-MM-dd"),
					_mainVm.EndDate.ToString("yyyy-MM-dd"));

				SummaryChart = Imaging.CreateBitmapSourceFromHBitmap(
					bitmap.GetHbitmap(),
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());

				return true;
			}
			catch (Exception e)
			{
				SummaryChartText = e.ToString();
				return false;
			}
		}

		#endregion //  Implementation
	}
}