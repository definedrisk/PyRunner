// ************************************************************************************************
// <copyright file="Stock.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace PyRunnerDemo.Core
{
	/// <summary>
	/// The application's stock entity.
	/// </summary>
	[Table("Stocks")]
	public class Stock
	{
		[Key, Column("Id", TypeName = "INTEGER")]
		public long Id { get; [UsedImplicitly] set; }

		[Column("Ticker", TypeName = "VARCHAR")]
		public string Ticker { get; [UsedImplicitly] set; }

		[Column("Name", TypeName = "VARCHAR")]
		public string Name { get; [UsedImplicitly] set; }
	}
}