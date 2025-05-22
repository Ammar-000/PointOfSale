using System.Linq.Expressions;

namespace Helper.Interfaces;

public interface IExpressionConverter<TSource, TTarget>
{
    Expression<Func<TTarget, bool>> Convert(Expression<Func<TSource, bool>> sourceExpression);
}