namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public class UserQueryParams
{
    private const int MaxPageSize = 50; 

    public int PageNumber { get; set; } = 1; 

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; } 
    public string? SortBy { get; set; } 
    public bool IsDescending { get; set; } = false; 
    public bool? IsActive { get; set; } 
}