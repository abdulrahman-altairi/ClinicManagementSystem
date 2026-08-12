namespace ClinicManagementSystem.Application.DTOs.Auth.Permissions; // تأكد من مطابقة الـ namespace لمجلد مشروعك

public record PermissionSearchFilter
{
    public string? SearchTerm { get; init; }

    public string? Module { get; init; }

    public bool? IsActive { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}