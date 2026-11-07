namespace SleveenAdmin.System.Bootstrap;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
