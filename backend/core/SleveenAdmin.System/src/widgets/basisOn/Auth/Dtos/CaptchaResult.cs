namespace SleveenAdmin.System.Auth.Dtos;

public sealed class CaptchaResult
{
    public string CaptchaId { get; set; } = string.Empty;

    /// <summary>data:image/svg+xml;base64,... 可直接用于 img src</summary>
    public string CaptchaImage { get; set; } = string.Empty;
}
