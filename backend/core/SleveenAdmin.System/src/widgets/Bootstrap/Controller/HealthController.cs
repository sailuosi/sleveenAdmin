using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.System.Bootstrap.Service;

namespace SleveenAdmin.System.Bootstrap.Controller;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthService _healthService;

    public HealthController(HealthService healthService) => _healthService = healthService;

    [HttpGet]
    [ApiResult]
    public async Task<object> Get(CancellationToken cancellationToken)
    {
        var (dbConnected, message) = await _healthService.CheckAsync(cancellationToken);
        return new
        {
            status = dbConnected ? "healthy" : "degraded",
            database = dbConnected,
            message,
            timestamp = DateTimeOffset.UtcNow
        };
    }
}
