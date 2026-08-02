namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateTime UtcNowDateTime { get; }

    DateOnly UtcToday { get; }
}
