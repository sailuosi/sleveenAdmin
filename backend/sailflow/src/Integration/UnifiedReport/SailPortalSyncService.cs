using SleveenAdmin.Foundation.Diagnostics;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Sailflow.Integration.UnifiedReport;

/// <summary>
/// stub: console-only; real port later。
/// 门户补推伪服务：仅控制台占位，不接 UnifiedReport HTTP。
/// </summary>
[Scoped]
public sealed class SailPortalSyncService
{
    public sealed class SyncResult
    {
        public int WaitingTaskCount { get; set; }
        public int Pushed { get; set; }
        public int AlreadyPushed { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool Stub { get; set; } = true;
    }

    /// <summary>按流程实例补推（伪）：打控制台后返回空成功结果。</summary>
    public Task<SyncResult> SyncByProcessInstanceAsync(string processInstanceId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var pi = processInstanceId?.Trim() ?? "";
        DevConsole.WriteBlock(
            "门户补推 · SyncPush",
            "stub: console-only; real port later",
            DevConsole.Field("流程实例", pi),
            DevConsole.Field("状态", string.IsNullOrEmpty(pi) ? "缺少 ProcessInstanceId" : "已占位（未真实推送）"));

        var result = new SyncResult();
        if (string.IsNullOrEmpty(pi))
            result.Errors.Add("ProcessInstanceId required");
        return Task.FromResult(result);
    }
}
