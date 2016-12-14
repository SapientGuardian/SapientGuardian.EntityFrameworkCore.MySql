using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.Extensions.Logging;

namespace SapientGuardian.MySql.Data.EntityFrameworkCore.Query.ExpressionTranslators
{
    public class MySqlContainsTranslator : ContainsTranslator
    {
        private static readonly MethodInfo MethodInfo = typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) });

        public MySqlContainsTranslator(ILogger logger) : base(logger) { }

        public override Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null) throw new ArgumentNullException(nameof(methodCallExpression));
            return methodCallExpression.Method != MethodInfo
                ? null
                : new LikeExpression(
                    methodCallExpression.Object,
                    methodCallExpression.Arguments[0]
                );
        }
    }
}
