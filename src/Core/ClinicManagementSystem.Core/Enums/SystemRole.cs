namespace ClinicManagementSystem.Domain.Enums;

public enum SystemRole
{
    SuperAdmin = 1,
    Admin = 2,
    Doctor = 3,
    Nurse = 4,
    Receptionist = 5,
    Patient = 6,
    Billing = 7,
    LabTechnician = 8,
    Pharmacist = 9,
    ReadOnly = 10
}
public static class SystemRoleExtensions
{
    public static string ToRoleName(this SystemRole role) => role.ToString();

    public static string ToNormalizedName(this SystemRole role)
        => role.ToString().ToUpperInvariant();
}
