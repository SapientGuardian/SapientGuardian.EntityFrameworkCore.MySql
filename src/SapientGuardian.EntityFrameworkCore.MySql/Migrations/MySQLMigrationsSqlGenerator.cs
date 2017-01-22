// Copyright © 2015, 2016 Oracle and/or its affiliates. All rights reserved.
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

using System;
using MySQL.Data.Entity.Metadata;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using JetBrains.Annotations;

namespace MySQL.Data.Entity.Migrations
{
	public class MySQLMigrationsSqlGenerator : MigrationsSqlGenerator
	{
		public MySQLMigrationsSqlGenerator(
			IRelationalCommandBuilderFactory commandBuilderFactory,
			ISqlGenerationHelper sqlGenerator,
			IRelationalTypeMapper typeMapper,
			IRelationalAnnotationProvider annotations)
				: base(commandBuilderFactory, sqlGenerator, typeMapper, annotations)
		{ }

		protected override void Generate(
			MigrationOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			if(operation is MySQLCreateDatabaseOperation)
				Generate(operation as MySQLCreateDatabaseOperation, model, builder);
			else if(operation is MySQLDropDatabaseOperation)
				Generate(operation as MySQLDropDatabaseOperation, model, builder);
			else
				base.Generate(operation, model, builder);
		}

		protected override void Generate(
			EnsureSchemaOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			throw new NotImplementedException();
		}

		protected override void Generate(
			AlterColumnOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			var columnType = operation.ColumnType;
			if(columnType == null)
			{
				var property = FindProperty(model, operation.Schema, operation.Table, operation.Name);
				columnType = property != null
					? TypeMapper.GetMapping(property).StoreType
					: TypeMapper.GetMapping(operation.ClrType).StoreType;
			}

			builder
				.Append("ALTER TABLE ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
				.Append(" MODIFY ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
				.Append(" ")
				.Append(columnType)
				.Append(operation.IsNullable ? " NULL" : " NOT NULL");

			if(operation.DefaultValue != null)
			{
				builder
					.Append(" DEFAULT ")
					.Append(SqlGenerationHelper.GenerateLiteral(operation.DefaultValue));
			}
			else if(!string.IsNullOrWhiteSpace(operation.ComputedColumnSql))
			{
				builder
					.Append(" DEFAULT ")
					.Append(operation.DefaultValueSql);
			}

			builder.AppendLine(SqlGenerationHelper.StatementTerminator);
		}

		protected override void Generate(
			RenameColumnOperation operation,
			IModel model,
			MigrationCommandListBuilder builder
		) 
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			var property = FindProperty(model, operation.Schema, operation.Table, operation.NewName);
            if (property != null)
            {
                var columnType = TypeMapper.GetMapping(property).StoreType;

                builder
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" CHANGE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .Append(" ")
                    .Append(columnType)
                    .Append(property.IsNullable ? " NULL" : " NOT NULL");

                builder.AppendLine(SqlGenerationHelper.StatementTerminator);
            }
		}

		protected override void Generate(
			DropIndexOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			builder
				.Append("DROP INDEX ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
				.Append(" ON ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
				.AppendLine(SqlGenerationHelper.StatementTerminator);
		}

		protected override void Generate(
			DropPrimaryKeyOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			throw new NotImplementedException();
		}

		protected override void Generate(
			DropForeignKeyOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			builder
				.Append("ALTER TABLE ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
				.Append(" DROP FOREIGN KEY ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
				.AppendLine(SqlGenerationHelper.StatementTerminator);
		}

        protected override void Generate(DropUniqueConstraintOperation operation, 
            IModel model, 
            MigrationCommandListBuilder builder)
        {
            ThrowIf.Argument.IsNull(operation, "operation");
            ThrowIf.Argument.IsNull(builder, "builder");

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected virtual void Generate(
			MySQLCreateDatabaseOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			builder
				.Append("CREATE DATABASE ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));
		}

		protected virtual void Generate(
			MySQLDropDatabaseOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(operation, "operation");
			ThrowIf.Argument.IsNull(builder, "builder");

			builder
				.Append("DROP DATABASE IF EXISTS ")
				.Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));
		}


		protected override void ColumnDefinition(
			string schema,
			string table,
			string name,
			Type clrType,
			string type,
			bool? unicode,
			int? maxLength,
			bool rowVersion,
			bool nullable,
			object defaultValue,
			string defaultValueSql,
			string computedColumnSql,
			IAnnotatable annotatable,
			IModel model,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsEmpty(name, "name");
			ThrowIf.Argument.IsNull(clrType, "clrType");
			ThrowIf.Argument.IsNull(annotatable, "annotatable");
			ThrowIf.Argument.IsNull(builder, "builder");

			if(computedColumnSql != null)
			{
				builder
					 .Append(SqlGenerationHelper.DelimitIdentifier(name))
					 .Append(string.Format(" {0} AS ", type))
					 .Append(" ( " + computedColumnSql + " )");

				return;

			}


			var autoInc = annotatable[MySQLAnnotationNames.Prefix + MySQLAnnotationNames.AutoIncrement];


			base.ColumnDefinition(
			schema,
			table,
			name,
			clrType,
			type,
			unicode,
			maxLength,
			rowVersion,
			nullable,
			defaultValue,
			defaultValueSql,
			computedColumnSql,
			annotatable,
			model,
			builder);

			if(autoInc != null && (bool)autoInc)
			{
				builder.Append(" AUTO_INCREMENT");
			}
		}


		protected override void DefaultValue(
			object defaultValue,
			string defaultValueSql,
			MigrationCommandListBuilder builder)
		{
			ThrowIf.Argument.IsNull(builder, nameof(builder));

			if(defaultValueSql != null)
			{
				builder
					.Append(" DEFAULT ")
					.Append(defaultValueSql);
			}
			else if(defaultValue != null)
			{
                if (defaultValue is DateTime)
                {
                    var dt = (DateTime)defaultValue;
                    builder
                       .Append(" DEFAULT ")
                       .Append(SqlGenerationHelper.GenerateLiteral(dt.ToString("yyyy-MM-dd HH:mm:ss")));                    
                }
                else
                {
                    builder
                        .Append(" DEFAULT ")
                        .Append(SqlGenerationHelper.GenerateLiteral(defaultValue.ToString()));
                }
            }
        }



		protected override void PrimaryKeyConstraint(
			AddPrimaryKeyOperation operation,
			IModel model,
			MigrationCommandListBuilder builder)
		{

			ThrowIf.Argument.IsNull(operation, "AddPrimaryKeyOperation");
			ThrowIf.Argument.IsNull(builder, "RelationalCommandListBuider");


			//MySQL always assign PRIMARY to the PK name no way to override that.
			// check http://dev.mysql.com/doc/refman/5.1/en/create-table.html


			builder
				.Append("PRIMARY KEY ")
				.Append("(")
				.Append(string.Join(", ", operation.Columns.Select(SqlGenerationHelper.DelimitIdentifier)))
				.Append(")");

			IndexTraits(operation, model, builder);
		}
	}
}
