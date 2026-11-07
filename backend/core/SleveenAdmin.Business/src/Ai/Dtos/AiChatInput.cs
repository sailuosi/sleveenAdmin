namespace SleveenAdmin.Business.Ai.Dtos;

public class AiChatInput
{
    public string? Prompt { get; set; }

    public List<AiChatMessageInput>? Messages { get; set; }
}
