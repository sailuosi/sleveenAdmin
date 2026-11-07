using SleveenAdmin.Foundation.Data;
using SleveenAdmin.System.Auth.DBLayer;

namespace SleveenAdmin.System.Jobs;

public sealed class RefreshTokenCleanupJob
{
    private readonly MooRepository<SysRefreshToken> _refreshTokens;

    public RefreshTokenCleanupJob(MooRepository<SysRefreshToken> refreshTokens) =>
        _refreshTokens = refreshTokens;

    public Task ExecuteAsync()
    {
        var expired = _refreshTokens.Repo.GetList(t =>
            t.ExpireTime < DateTime.Now || t.IsRevoked);

        foreach (var token in expired)
            _refreshTokens.Repo.Delete(token);

        return Task.CompletedTask;
    }
}
