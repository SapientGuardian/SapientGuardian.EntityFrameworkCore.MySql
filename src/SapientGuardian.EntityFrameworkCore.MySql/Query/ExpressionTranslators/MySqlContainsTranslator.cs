using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace SapientGuardian.MySql.Data.EntityFrameworkCore.Query.ExpressionTranslators
{
    public class MySqlContainsTranslator : ContainsTranslator
    {
        private static readonly MethodInfo MethodInfo = typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) });

        private static readonly MethodInfo Format = typeof(string).GetRuntimeMethod("Format", new[]
        {
              typeof (string),
              typeof (object)
            });

        public override Expression Translate(MethodCallExpression methodCallExpression)
        {
            //    Expression.Coalesce(Expression.Add(
            //                Expression.Add(Expression.Constant("%", typeof(string)),
            //                    methodCallExpression.Arguments[0], Concat),
            //                Expression.Constant("{0}%", typeof(string)), Concat),
            //    Expression.Constant(string.Empty, typeof(string)))
            if (methodCallExpression == null) throw new ArgumentNullException(nameof(methodCallExpression));
            return methodCallExpression.Method != MethodInfo
                ? null
                : new LikeExpression(
                    methodCallExpression.Object,
                    Expression.Call(Format, Expression.Constant("%{0}%", typeof(string)), methodCallExpression.Arguments[0])
                );
        }
    }
}
