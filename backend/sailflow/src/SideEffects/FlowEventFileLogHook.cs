using System.Text;
using System.Text.Json;
using sailflow.engine.Hooks;
using sailflow.msg.SideEffect;

namespace SleveenAdmin.Sailflow.SideEffects;

/// <summary>
/// BestEffort：流程事件串行追加写文件。主路径只入 Channel；满队可丢；崩溃不补。
/// </summary>
public sealed class FlowEventFileLogHook : BestEffortFlowHookBase<string>
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _directory;
    private readonly string _filePrefix;

    public FlowEventFileLogHook(string directory = null, string filePrefix = "flow-events", BestEffortChannelOptions channelOptions = null)
        : base(channelOptions ?? new BestEffortChannelOptions { Capacity = 8192, DropWhenFull = true })
    {
        _directory = string.IsNullOrWhiteSpace(directory)
            ? Path.Combine(AppContext.BaseDirectory, "logs", "sail-side-effects")
            : directory;
        _filePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "flow-events" : filePrefix.Trim();
    }

    public override string Name => "sleveen.flow.event.filelog";

    protected override bool TryCreatePayload(FlowHookContext ctx, out string payload)
    {
        payload = null;
        if (ctx == null)
            return false;

        try
        {
            var line = new StringBuilder(256);
            line.Append(DateTime.UtcNow.ToString("O"));
            line.Append(' ');
            line.Append(JsonSerializer.Serialize(ctx, JsonOpts));
            payload = line.ToString();
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override Task ConsumeAsync(string payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(payload))
            return Task.CompletedTask;

        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, $"{_filePrefix}-{DateTime.UtcNow:yyyyMMdd}.log");
        // 单消费者串行：无需 lock；并发 Append 会竞争文件
        File.AppendAllText(path, payload + Environment.NewLine, Encoding.UTF8);
        return Task.CompletedTask;
    }
}
