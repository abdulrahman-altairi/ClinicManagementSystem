using System.Data;

namespace ClinicManagementSystem.Application.Common.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();

    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
