namespace ToolInventory.MAUI.Services;

public class ApiResult
{
    public bool IsSuccess { get; init; }
    public int? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult Success() => new() { IsSuccess = true };

    public static ApiResult Fail(int? statusCode, string? errorMessage)
        => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "The request failed." : errorMessage
        };
}

public sealed class ApiResult<T> : ApiResult
{
    public T? Value { get; init; }

    public static ApiResult<T> Success(T value)
        => new()
        {
            IsSuccess = true,
            Value = value
        };

    public new static ApiResult<T> Fail(int? statusCode, string? errorMessage)
        => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "The request failed." : errorMessage
        };
}
