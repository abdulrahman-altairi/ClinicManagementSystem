namespace ClinicManagementSystem.Application.DTOs.Auth.Role; 

public record RoleSearchFilter
{
    public string? SearchTerm { get; init; } 

    public bool? IsActive { get; init; }     

    public bool? IsSystem { get; init; }     

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; } = "CreatedAt";

    public string? SortDirection { get; init; } = "DESC";
}