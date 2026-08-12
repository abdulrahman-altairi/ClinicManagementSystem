namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public class AdminUpdateUserRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } 
    public bool LockoutEnabled { get; set; } 

    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}