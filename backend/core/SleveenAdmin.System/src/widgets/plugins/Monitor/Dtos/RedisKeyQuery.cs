using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.System.Monitor.Dtos;

public sealed class RedisKeyQuery : PageQuery
{
    public string? Pattern { get; set; }
}
