namespace SleveenAdmin.System.Auth.Dtos;

public class LoginInput
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? CaptchaId { get; set; }

    public string? CaptchaCode { get; set; }
}
