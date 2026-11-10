using Microsoft.Extensions.AI;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Business.Ai;

[Singleton]
public sealed class PlaceholderChatClient : IChatClient
{
    public ChatClientMetadata Metadata { get; } = new("placeholder");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var reply = "sleveen-admin AI placeholder: configure Ai:Provider and API key to enable a real model.";
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const string text = "sleveen-admin AI streaming placeholder. Configure Ai section to connect a real model.";
        foreach (var chunk in text.Split(' '))
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk + " ");
            await Task.Delay(30, cancellationToken);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
