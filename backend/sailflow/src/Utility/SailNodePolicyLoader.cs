using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Utility;

/// <summary>从 Runner / 任务上下文加载当前节点 NodePolicy。</summary>
public static class SailNodePolicyLoader
{
    public static SailNodePolicyDto LoadForRunner(SailClient sail, FlowCommandContext? runner)
    {
        if (sail == null || runner == null)
            return SailNodePolicyCatalog.Merge(null);

        try
        {
            string? activityId = null;
            string? processId = runner.ProcessId;
            string? version = runner.Version;

            if (!string.IsNullOrWhiteSpace(runner.TaskId))
            {
                var view = new WorkflowService(sail).GetTaskView(runner.TaskId);
                if (view != null)
                {
                    if (!string.IsNullOrWhiteSpace(view.ActivityId))
                        activityId = view.ActivityId;
                    if (string.IsNullOrWhiteSpace(processId))
                        processId = view.ProcessId;
                    if (string.IsNullOrWhiteSpace(version))
                        version = view.Version;
                }
            }

            activityId ??= FirstNextActivityId(runner);

            if (string.IsNullOrWhiteSpace(processId) || string.IsNullOrWhiteSpace(activityId))
                return SailNodePolicyCatalog.Merge(null);

            var ver = string.IsNullOrWhiteSpace(version) ? "1" : version!;
            var file = new WorkflowService(sail).GetProcessFile(processId, ver);
            return SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, activityId);
        }
        catch
        {
            return SailNodePolicyCatalog.Merge(null);
        }
    }

    public static SailNodePolicyDto LoadForActivity(
        SailClient sail,
        string? processId,
        string? version,
        string? activityId)
    {
        if (string.IsNullOrWhiteSpace(processId) || string.IsNullOrWhiteSpace(activityId))
            return SailNodePolicyCatalog.Merge(null);
        try
        {
            var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
            var file = new WorkflowService(sail).GetProcessFile(processId, ver);
            return SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, activityId);
        }
        catch
        {
            return SailNodePolicyCatalog.Merge(null);
        }
    }

    static string? FirstNextActivityId(FlowCommandContext runner)
    {
        var keys = runner.NextActivityPerformers?.Keys;
        return keys?.FirstOrDefault(k => !string.IsNullOrWhiteSpace(k));
    }
}
