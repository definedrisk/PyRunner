// ************************************************************************************************
// <copyright file="TreeViewViewModel.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PyRunnerDemo.UI
{
	public class TreeViewViewModel : ViewModelBase
	{
		#region Constants

		public const string TreeNeedsRefresh = "Refresh required.";
		public const string Processing = "Processing ...";
		public const string Waiting = "...";
		public const string ErrorNumTooHigh = "Number of clusters\nmust not exceed\nnumber of stocks!";

		#endregion //  Constants

		#region Fields

		private readonly MainViewModel _mainVm;

		private string _treeViewText;

		#endregion //  Fields

		#region Construction

		public TreeViewViewModel(MainViewModel mainViewModel, string kMeansScript)
		{
			_mainVm = mainViewModel;
			KMeansClusteringScript = kMeansScript;

			TreeViewText = Waiting;
			Items = new ObservableCollection<TreeViewItem>();
		}

		#endregion //  Construction

		#region Properties

		internal string KMeansClusteringScript { get; }

		/// <summary>
		/// A text that is displayed in some situations instead of the treeview.
		/// </summary>
		public string TreeViewText
		{
			get => _treeViewText;
			set
			{
				if (_treeViewText != value)
				{
					_treeViewText = value;
					OnPropertyChanged();
				}
			}
		}

		public ObservableCollection<TreeViewItem> Items { get; }

		#endregion //  Properties

		#region Implementation

		internal async Task<bool> PerformKMeans()
		{
			if (_mainVm.NumClusters > _mainVm.SelectedItems.Count)
			{
				TreeViewText = ErrorNumTooHigh;
				return false;
			}

			TreeViewText = Processing;

			try
			{
				string list = await RunKMeans();

				if (string.IsNullOrWhiteSpace(list))
				{
					return false;
				}

				string[] lines = list.Split(new []{ Environment.NewLine}, StringSplitOptions.None);

				Items.Clear();

				for (byte i = 0; i < _mainVm.NumClusters; i++)
				{
					var clusterItem = new TreeViewItem
					{
						Header = $"Cluster {i}",
						IsExpanded = true,
						FontWeight = FontWeights.Bold,
						FontSize = 14
					};

					foreach (string line in lines.Where(l => l.StartsWith(i.ToString())))
					{
						clusterItem.Items.Add(CreateItem(line));
					}

					Items.Add(clusterItem);
				}

				return true;
			}
			catch (Exception e)
			{
				TreeViewText = e.ToString();
				return false;
			}
		}

		private static TreeViewItem CreateItem(string line)
		{
			string ticker = line.Split(' ')[1];
			byte[] rgb = line.Split(' ')[2].Split(',').Select(s => Convert.ToByte(s)).ToArray();

			var panel = new StackPanel {Orientation = Orientation.Horizontal};

			panel.Children.Add(new Line
			{
				Stroke = new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2])),
				X1 = 1,
				X2 = 30,
				Y1 = 1,
				Y2 = 1,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				StrokeThickness = 2,
				Width = 40
			});
			panel.Children.Add(new TextBlock
			{
				Text = ticker
			});

			return new TreeViewItem
			{
				Header = panel,
				FontWeight = FontWeights.Normal,
				FontSize = 12
			};
		}

		/// <summary>
		/// Calls the python script to retrieve a textual list that 
		/// will subsequently be used for building the treeview.
		/// </summary>
		/// <returns>True on success.</returns>
		private async Task<string> RunKMeans()
		{
			TreeViewText = Processing;
			Items.Clear();

			try
			{
				string output = await _mainVm.PythonRunner.ExecuteAsync(
					KMeansClusteringScript,
					_mainVm.DbPath,
					_mainVm.TickerList,
					_mainVm.NumClusters,
					_mainVm.StartDate.ToString("yyyy-MM-dd"),
					_mainVm.EndDate.ToString("yyyy-MM-dd"));

				return output;
			}
			catch (Exception e)
			{
				TreeViewText = e.ToString();
				return string.Empty;
			}
		}

		#endregion //  Implementation
	}
}