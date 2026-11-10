using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.System.Bootstrap.Service;

[Scoped]
public sealed class HealthService {
    private readonly IDbInitializer _dbInitializer;

    public HealthService(IDbInitializer dbInitializer) => _dbInitializer = dbInitializer;

    public async Task<(bool DbConnected, string Message)> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbInitializer.InitializeAsync(cancellationToken);
            return (true, "SQL Server connected");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
