namespace ClinicManagementSystem.Application.Common.Models;

public sealed class ErrorModel
{
    public string Field { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public ErrorModel() { }

    public ErrorModel(string field, string message, string code = "")
    {
        Field = field;
        Message = message;
        Code = code;
    }

    public static ErrorModel Global(string message, string code = "")
        => new(string.Empty, message, code);
}
