namespace Helper.DataContainers;

public class OperationResult
{
    public bool Succeeded { get; protected set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static OperationResult Success(string message = "") => new()
    {
        Succeeded = true,
        Message = message
    };

    public static OperationResult Fail(string error) => new()
    {
        Succeeded = false,
        Errors = new() { error }
    };

    public static OperationResult Fail(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };

    public static OperationResult Fail(List<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };

    public void AddError(string error)
    {
        Succeeded = false;
        Errors.Add(error);
    }
}


public class OperationResult<T>:OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Success(T? data, string message = "") => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public static new OperationResult<T> Fail(string error) => new()
    {
        Succeeded = false,
        Errors = new() { error }
    };

    public static new OperationResult<T> Fail(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };

    public static new OperationResult<T> Fail(List<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };
}
