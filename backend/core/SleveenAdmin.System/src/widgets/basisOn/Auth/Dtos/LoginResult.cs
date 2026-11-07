namespace SleveenAdmin.System.Auth.Dtos;

public sealed class LoginResult
{
    public string Token { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
