using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Business.Ai.Dtos;
using SleveenAdmin.Business.Ai.Service;

namespace SleveenAdmin.Business.Ai.Controller;

[Authorize]
[ApiController]
[Route("api/ai")]
public class AiChatController : ControllerBase
{
    private readonly AiChatService _aiChatService;

    public AiChatController(AiChatService aiChatService) => _aiChatService = aiChatService;

    [HttpPost("chat")]
    public async Task Chat([FromBody] AiChatInput input, CancellationToken cancellationToken)
    {
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-transform";
        Response.Headers.Connection = "keep-alive";
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var chunk in _aiChatService.StreamChatAsync(input, cancellationToken))
        {
            await Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
