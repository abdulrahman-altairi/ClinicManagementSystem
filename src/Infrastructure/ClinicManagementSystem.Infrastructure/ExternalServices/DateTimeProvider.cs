
using ClinicManagementSystem.Application.Common.Interfaces;

namespace ClinicManagementSystem.Infrastructure.ExternalServices;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTime UtcNowDateTime => DateTime.UtcNow;

    public DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);
}