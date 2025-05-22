using Helper.Interfaces;
using System.Linq.Expressions;

namespace Helper.Implementations;

public class ExpressionCombiner<T> : IExpressionCombiner<T>
{
    public Expression<Func<T, bool>> And(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T));

        BinaryExpression body = Expression.AndAlso(
            ReplaceParameter(expr1.Body, expr1.Parameters[0], parameter),
            ReplaceParameter(expr2.Body, expr2.Parameters[0], parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public Expression<Func<T, bool>> Or(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T));

        BinaryExpression body = Expression.OrElse(
            ReplaceParameter(expr1.Body, expr1.Parameters[0], parameter),
            ReplaceParameter(expr2.Body, expr2.Parameters[0], parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    Expression ReplaceParameter(Expression body, ParameterExpression toReplace, ParameterExpression replaceWith)
    {
        return new ParameterReplacer(toReplace, replaceWith).Visit(body);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
}
