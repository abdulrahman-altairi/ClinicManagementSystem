namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public class UpdateUserProfileRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    
}