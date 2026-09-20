namespace Application.Results;

public class Result
{
    public ResultStatus Status { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Status == ResultStatus.Ok;

    public static Result Success() => new() { Status = ResultStatus.Ok };
    public static Result NotFound(string? error = null) => new() { Status = ResultStatus.NotFound, Error = error };
    public static Result Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static Result Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
}
