using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace SleveenAdmin.Business.Ai;

public static class AiSetup
{
    public static IServiceCollection AddSleveenAdminAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiOptions>()
            .Configure(options =>
            {
                configuration.GetSection(AiOptions.SectionName).Bind(options);
                ApplyLlmProviderOverrides(options, configuration);
            });
        services.AddSingleton<PlaceholderChatClient>();
        services.AddSingleton<AIAgent>(sp => CreateAgent(sp));
        return services;
    }

    /// <summary>
    /// 读取可选 ModelProvider 节（如旧路径 Configration/llm.json），覆盖 Ai 节中的密钥与端点。
    /// </summary>
    internal static void ApplyLlmProviderOverrides(AiOptions options, IConfiguration configuration)
    {
        var apiKey = configuration["ModelProvider:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return;

        options.Provider = AiProviders.OpenAI;
        options.ApiKey = apiKey;

        var endpoint = configuration["ModelProvider:EndPoint"]
            ?? configuration["ModelProvider:Endpoint"];
        if (!string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = endpoint;

        var modelId = configuration["ModelProvider:ModelId"];
        if (!string.IsNullOrWhiteSpace(modelId))
            options.Model = modelId;
    }

    private static AIAgent CreateAgent(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
        var instructions = string.IsNullOrWhiteSpace(options.Instructions)
            ? "你是 sleveen-admin 后台管理系统的智能助手，请用简洁、准确的中文回答用户问题。"
            : options.Instructions.Trim();
        var name = string.IsNullOrWhiteSpace(options.AgentName) ? "sleveen-admin助手" : options.AgentName.Trim();

        if (string.Equals(options.Provider, AiProviders.OpenAI, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(options.ApiKey))
        {
            var chatClient = SleveenAdminAiClient.CreateOpenAiChatClient(options);
            return chatClient.AsAIAgent(instructions: instructions, name: name);
        }

        var placeholder = sp.GetRequiredService<PlaceholderChatClient>();
        return placeholder.AsAIAgent(new ChatClientAgentOptions
        {
            Name = name,
            ChatOptions = new ChatOptions { Instructions = instructions }
        });
    }
}
