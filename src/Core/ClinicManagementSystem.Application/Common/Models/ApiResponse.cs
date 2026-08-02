namespace ClinicManagementSystem.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public T? Data { get; private init; }
    public List<ErrorModel> Errors { get; private init; } = new();
    public DateTimeOffset Timestamp { get; private init; } = DateTimeOffset.UtcNow;

    private ApiResponse() { }

    public static ApiResponse<T> Success(T data, string message = "Operation completed successfully.")
        => new() { IsSuccess = true, Message = message, Data = data };

    public static ApiResponse<T> Failure(string message, List<ErrorModel>? errors = null)
        => new() { IsSuccess = false, Message = message, Errors = errors ?? new List<ErrorModel>() };

    public static ApiResponse<T> Failure(string message, ErrorModel error)
        => Failure(message, new List<ErrorModel> { error });
}
