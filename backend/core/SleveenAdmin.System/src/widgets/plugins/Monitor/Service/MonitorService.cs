using System.Diagnostics;
using System.Runtime.InteropServices;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.System.Monitor.Dtos;

namespace SleveenAdmin.System.Monitor.Service;

public sealed class MonitorService : IScoped
{
    private static readonly Process CurrentProcess = Process.GetCurrentProcess();

    public ServerInfoDto GetServerInfo()
    {
        var uptime = DateTime.UtcNow - CurrentProcess.StartTime.ToUniversalTime();
        return new ServerInfoDto
        {
            MachineName = Environment.MachineName,
            OsDescription = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            ProcessorCount = Environment.ProcessorCount,
            WorkingSetBytes = CurrentProcess.WorkingSet64,
            GcHeapBytes = GC.GetTotalMemory(false),
            ProcessUptime = uptime,
            Disks = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new DiskInfoDto
                {
                    Name = d.Name,
                    DriveType = d.DriveType.ToString(),
                    TotalBytes = d.TotalSize,
                    FreeBytes = d.AvailableFreeSpace,
                    UsedPercent = d.TotalSize <= 0
                        ? 0
                        : Math.Round((d.TotalSize - d.AvailableFreeSpace) * 100.0 / d.TotalSize, 2)
                })
                .ToList()
        };
    }
}
