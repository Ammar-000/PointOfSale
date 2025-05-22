using System.Linq.Expressions;

namespace Helper.Interfaces;

public interface IExpressionCombiner<T>
{
    public Expression<Func<T, bool>> And(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2);
    public Expression<Func<T, bool>> Or(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2);
}
