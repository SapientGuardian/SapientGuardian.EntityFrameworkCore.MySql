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

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using SapientGuardian.MySql.Data.EntityFrameworkCore.Query.ExpressionTranslators;
using System.Linq;

namespace MySQL.Data.Entity
{
  public class MySQLCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        public MySQLCompositeMethodCallTranslator(ILogger<MySQLCompositeMethodCallTranslator> logger) : base(logger)
        {
            this._methodCallTranslators = new List<IMethodCallTranslator>()
            {
                new MySqlContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(logger),
                new IsNullOrEmptyTranslator(),
                new StartsWithTranslator()
            };
        }

        public override Expression Translate(MethodCallExpression methodCallExpression)
        {
            return this._methodCallTranslators.Select(translator => translator.Translate(methodCallExpression))
                .FirstOrDefault(translatedMethodCall => translatedMethodCall != null);
        }


        private readonly List<IMethodCallTranslator> _methodCallTranslators;

        /// <summary>Adds additional translators to the dispatch list.</summary>
        /// <param name="translators"> The translators. </param>
        protected override void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
        {
            this._methodCallTranslators.AddRange(translators);
        }
    }
}
