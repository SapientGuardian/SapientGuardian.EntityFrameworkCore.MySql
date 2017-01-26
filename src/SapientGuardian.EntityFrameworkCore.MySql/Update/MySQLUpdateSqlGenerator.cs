﻿// Copyright © 2015, Oracle and/or its affiliates. All rights reserved.
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

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using System.Text;
using System.Collections.Generic;

namespace MySQL.Data.Entity
{
	public class MySQLUpdateSqlGenerator : UpdateSqlGenerator
	{
		public MySQLUpdateSqlGenerator(ISqlGenerationHelper sqlGenerator)
				: base(sqlGenerator)
		{ }

		protected override void AppendValues(StringBuilder commandStringBuilder, IReadOnlyList<ColumnModification> operations) 
		{ 
			ThrowIf.Argument.IsNull(commandStringBuilder, "commandStringBuilder"); 
			ThrowIf.Argument.IsNull(operations, "operations"); 
 
			if (operations.Count > 0) 
			{ 
				base.AppendValues(commandStringBuilder, operations); 
			} 
			else 
			{ 
				commandStringBuilder.Append("()"); 
			} 
		} 
 
		protected override void AppendValuesHeader(StringBuilder commandStringBuilder, IReadOnlyList<ColumnModification> operations) 
		{ 
			ThrowIf.Argument.IsNull(commandStringBuilder, "commandStringBuilder"); 
 			ThrowIf.Argument.IsNull(operations, "operations"); 
 
			commandStringBuilder.AppendLine(); 
			commandStringBuilder.Append("VALUES "); 
		} 

		protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
		{
			ThrowIf.Argument.IsNull(columnModification, "columnModification");
			ThrowIf.Argument.IsNull(commandStringBuilder, "commandStringBuilder");
			commandStringBuilder.AppendFormat("{0}=LAST_INSERT_ID()", SqlGenerationHelper.DelimitIdentifier(columnModification.ColumnName));
		}


		protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
		{
			ThrowIf.Argument.IsNull(commandStringBuilder, "commandStringBuilder");
			commandStringBuilder
			  .Append("ROW_COUNT() = " + expectedRowsAffected)
			  .AppendLine();
		}

		protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
		{
			ThrowIf.Argument.IsNull(commandStringBuilder, "commandStringBuilder");
			commandStringBuilder
			  .Append("SELECT ROW_COUNT()")
			  .Append(SqlGenerationHelper.StatementTerminator)
			  .AppendLine();

			return ResultSetMapping.LastInResultSet;
		}

		public enum ResultsGrouping
		{
			OneResultSet,
			OneCommandPerResultSet
		}
	}
}
