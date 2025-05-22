using Helper.DataContainers;
using Helper.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Helper.Implementations;

public class EntityValidator<T> : IEntityValidator<T>
{
    public OperationResult ValidateEntity(T entity)
    {
        if (entity == null) return OperationResult.Fail("Entity is null");

        List<ValidationResult> validationResults = new();
        ValidationContext validationContext = new(entity, null, null);
        Validator.TryValidateObject(entity, validationContext, validationResults, true);

        return validationResults.Count == 0
            ? OperationResult.Success()
            : OperationResult.Fail(validationResults.Select(vr => vr.ErrorMessage ?? "Empty error message").ToList());
    }
}