using System.ComponentModel.DataAnnotations;

namespace Helper.CustomDataAnnotationsAttribut;

public class MinCollectionCountAttribute<T> : ValidationAttribute
{
    readonly int _minCount;

    public MinCollectionCountAttribute(int minCount)
    {
        _minCount = minCount;
        ErrorMessage = $"The collection must contains at least {_minCount} item(s).";
    }

    public override bool IsValid(object? value)
    {
        ICollection<T>? collection = value as ICollection<T>;
        return collection != null && collection.Count >= _minCount;
    }
}
