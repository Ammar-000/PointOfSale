using Helper.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace Helper.Implementations;

public class ExpressionConverter<TSource, TTarget> : ExpressionVisitor, IExpressionConverter<TSource, TTarget>
{
    ParameterExpression _sourceParameter, _targetParameter;

    void InitConverter(ParameterExpression sourceParameter)
    {
        _sourceParameter = sourceParameter;
        _targetParameter = Expression.Parameter(typeof(TTarget), sourceParameter.Name);
    }

    public Expression<Func<TTarget, bool>> Convert(Expression<Func<TSource, bool>> sourceExpression)
    {
        InitConverter(sourceExpression.Parameters[0]);
        Expression body = Visit(sourceExpression.Body);
        return Expression.Lambda<Func<TTarget, bool>>(body, _targetParameter);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _sourceParameter ? _targetParameter : base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression == _sourceParameter)
        {
            // Map member from source type to target type
            MemberInfo? targetMember = typeof(TTarget).GetMember(node.Member.Name).FirstOrDefault();
            if (targetMember is PropertyInfo)
                return Expression.Property(_targetParameter, targetMember.Name);
        }

        return base.VisitMember(node);
    }
}
