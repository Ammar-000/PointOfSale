using Helper.DataContainers;

namespace Helper.Interfaces;

public interface IEntityValidator<T>
{
    OperationResult ValidateEntity(T entity);
}
