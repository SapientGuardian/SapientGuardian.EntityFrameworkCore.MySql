// Copyright © 2015, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Data.Common;

namespace MySQL.Data.Entity.Extensions
{
	/// <summary>
	///     MySQL specific extension methods for <see cref="DbContextOptionsBuilder"/>.
	/// </summary>
	public static class MySQLDbContextOptionsExtensions
	{
		/// <summary>
		///     Configures the context to connect to a MySQL database.
		/// </summary>
		/// <param name="optionsBuilder"> The builder being used to configure the context. </param>
		/// <param name="connectionString"> The connection string of the database to connect to. </param>
		/// <param name="mysqlOptionsAction">An optional action to allow additional MySQL specific configuration.</param>
		/// <returns> The options builder so that further configuration can be chained. </returns>
		public static DbContextOptionsBuilder UseMySQL(
			this DbContextOptionsBuilder optionsBuilder,
			string connectionString,
			Action<MySQLDbContextOptionsBuilder> mysqlOptionsAction = null)
		{
			var extension = GetOrCreateExtension(optionsBuilder);
			extension.ConnectionString = connectionString;

			IDbContextOptionsBuilderInfrastructure o = optionsBuilder as IDbContextOptionsBuilderInfrastructure;
			o.AddOrUpdateExtension(extension);

			ConfigureWarnings(optionsBuilder);

			mysqlOptionsAction?.Invoke(new MySQLDbContextOptionsBuilder(optionsBuilder));

			return optionsBuilder;
		}

		/// <summary>
		///     Configures the context to connect to a MySQL database.
		/// </summary>
		/// <param name="optionsBuilder"> The builder being used to configure the context. </param>
		/// <param name="connection">
		///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
		///     in the open state then EF will not open or close the connection. If the connection is in the closed
		///     state then EF will open and close the connection as needed.
		/// </param>
		/// <param name="mysqlOptionsAction">An optional action to allow additional MySQL specific configuration.</param>
		/// <returns> The options builder so that further configuration can be chained. </returns>
		public static DbContextOptionsBuilder UseMySQL(
			this DbContextOptionsBuilder optionsBuilder,
			DbConnection connection,
			Action<MySQLDbContextOptionsBuilder> mysqlOptionsAction = null)
		{
			var extension = GetOrCreateExtension(optionsBuilder);
			extension.Connection = connection;

			IDbContextOptionsBuilderInfrastructure o = optionsBuilder as IDbContextOptionsBuilderInfrastructure;
			o.AddOrUpdateExtension(extension);

			ConfigureWarnings(optionsBuilder);

			mysqlOptionsAction?.Invoke(new MySQLDbContextOptionsBuilder(optionsBuilder));

			return optionsBuilder;
		}

		/// <summary>
		///     Configures the context to connect to a MySQL database.
		/// </summary>
		/// <typeparam name="TContext"> The type of context to be configured. </typeparam>
		/// <param name="optionsBuilder"> The builder being used to configure the context. </param>
		/// <param name="connectionString"> The connection string of the database to connect to. </param>
		/// <param name="mysqlOptionsAction">An optional action to allow additional MySQL specific configuration.</param>
		/// <returns> The options builder so that further configuration can be chained. </returns>
		public static DbContextOptionsBuilder<TContext> UseMySQL<TContext>(
			this DbContextOptionsBuilder<TContext> optionsBuilder,
			string connectionString,
			Action<MySQLDbContextOptionsBuilder> mysqlOptionsAction = null)
			where TContext : DbContext
			=> (DbContextOptionsBuilder<TContext>)UseMySQL(
				(DbContextOptionsBuilder)optionsBuilder, connectionString, mysqlOptionsAction);

		/// <summary>
		///     Configures the context to connect to a MySQL database.
		/// </summary>
		/// <typeparam name="TContext"> The type of context to be configured. </typeparam>
		/// <param name="optionsBuilder"> The builder being used to configure the context. </param>
		/// <param name="connection">
		///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
		///     in the open state then EF will not open or close the connection. If the connection is in the closed
		///     state then EF will open and close the connection as needed.
		/// </param>
		/// <param name="mysqlOptionsAction">An optional action to allow additional MySQL specific configuration.</param>
		/// <returns> The options builder so that further configuration can be chained. </returns>
		public static DbContextOptionsBuilder<TContext> UseMySQL<TContext>(
			this DbContextOptionsBuilder<TContext> optionsBuilder,
			DbConnection connection,
			Action<MySQLDbContextOptionsBuilder> mysqlOptionsAction = null)
			where TContext : DbContext
			=> (DbContextOptionsBuilder<TContext>)UseMySQL(
				(DbContextOptionsBuilder)optionsBuilder, connection, mysqlOptionsAction);


		private static MySQLOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
		{
			var existing = optionsBuilder.Options.FindExtension<MySQLOptionsExtension>();
			return existing != null
				? new MySQLOptionsExtension(existing)
				: new MySQLOptionsExtension();
		}

		private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
		{
			// Set warnings defaults
			optionsBuilder.ConfigureWarnings(w =>
			{
				w.Configuration.TryAddExplicit(
					RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw);
			});
		}
	}
}