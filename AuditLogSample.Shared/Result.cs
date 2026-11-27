namespace AuditLogSample.Shared;

public enum ErrorType
{
    None = 0,
    Validation = 400,
    NotFound = 404,
    Conflict = 409
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public ErrorType ErrorType { get; init; } = ErrorType.None;
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static Result Success(string message = "Success") =>
        new() { IsSuccess = true, Message = message };

    public static Result ValidationError(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.Validation, Errors = errors ?? [message] };

    public static Result NotFound(string message) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.NotFound, Errors = [message] };

    public static Result Conflict(string message) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.Conflict, Errors = [message] };
}

public sealed class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data, string message = "Success") =>
        new() { IsSuccess = true, Data = data, Message = message };

    public new static Result<T> ValidationError(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.Validation, Errors = errors ?? [message] };

    public new static Result<T> NotFound(string message) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.NotFound, Errors = [message] };

    public new static Result<T> Conflict(string message) =>
        new() { IsSuccess = false, Message = message, ErrorType = ErrorType.Conflict, Errors = [message] };
}
