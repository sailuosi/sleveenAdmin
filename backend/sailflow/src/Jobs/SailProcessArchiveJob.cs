using Microsoft.Extensions.Logging;
using sailflow.data.Hosting;
using sailflow.engine.Client;

namespace SleveenAdmin.Sailflow.Jobs;

/// <summary>Hangfire：按 DataLifecycleOptions 保留窗批量热→冷归档。</summary>
public sealed class SailProcessArchiveJob
{
    private readonly SailClient _sail;
    private readonly ILogger<SailProcessArchiveJob> _log;

    public SailProcessArchiveJob(SailClient sail, ILogger<SailProcessArchiveJob> log)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public Task ExecuteAsync()
    {
        var n = _sail.RunProcessArchiveBatch();
        if (n > 0)
            _log.LogInformation("Sail process archive batch moved {Count} instance(s).", n);
        return Task.CompletedTask;
    }
}
