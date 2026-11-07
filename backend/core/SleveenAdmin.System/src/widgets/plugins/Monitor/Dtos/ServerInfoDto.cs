namespace SleveenAdmin.System.Monitor.Dtos;

public sealed class ServerInfoDto
{
    public string MachineName { get; set; } = string.Empty;

    public string OsDescription { get; set; } = string.Empty;

    public string OsArchitecture { get; set; } = string.Empty;

    public string FrameworkDescription { get; set; } = string.Empty;

    public int ProcessorCount { get; set; }

    public long WorkingSetBytes { get; set; }

    public long GcHeapBytes { get; set; }

    public TimeSpan ProcessUptime { get; set; }

    public IReadOnlyList<DiskInfoDto> Disks { get; set; } = [];
}

public sealed class DiskInfoDto
{
    public string Name { get; set; } = string.Empty;

    public string DriveType { get; set; } = string.Empty;

    public long TotalBytes { get; set; }

    public long FreeBytes { get; set; }

    public double UsedPercent { get; set; }
}
