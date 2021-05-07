// ************************************************************************************************
// <copyright file="SQLiteConfiguration.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace PyRunnerDemo.Persistence
{
	/// <summary>
	/// For using SQLite with EF6. Copied from the Codeproject article 
	/// <see href="https://www.codeproject.com/Articles/1158937/SQLite-with-Csharp-Net-and-Entity-Framework">
	/// SQLite with C#.Net and Entity Framework</see>.
	/// </summary>
	public class SQLiteConfiguration : DbConfiguration
	{
		public SQLiteConfiguration()
		{
			SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
			SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
			SetProviderServices(
				"System.Data.SQLite",
				(DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
		}
	}
}