namespace Application.Results;

public class Result<T>
{
    public ResultStatus Status { get; init; }
    public string? Error { get; init; }
    public T? Value { get; init; }
    public bool IsSuccess => Status == ResultStatus.Ok;

    public static Result<T> Success(T value) => new() { Status = ResultStatus.Ok, Value = value };
    public static Result<T> NotFound(string? error = null) => new() { Status = ResultStatus.NotFound, Error = error };
    public static Result<T> Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static Result<T> Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
}
