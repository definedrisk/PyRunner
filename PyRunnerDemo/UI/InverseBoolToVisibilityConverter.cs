// ************************************************************************************************
// <copyright file="InverseBoolToVisibilityConverter.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PyRunnerDemo.UI
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class InverseBoolToVisibilityConverter : IValueConverter
	{
		public Visibility TrueValue { get; set; } = Visibility.Hidden;
		public Visibility FalseValue { get; set; } = Visibility.Visible;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			!(value is bool) ? (object) null : (bool) value ? TrueValue : FalseValue;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (Equals(value, TrueValue))
			{
				return true;
			}

			if (Equals(value, FalseValue))
			{
				return false;
			}

			return null;
		}
	}
}