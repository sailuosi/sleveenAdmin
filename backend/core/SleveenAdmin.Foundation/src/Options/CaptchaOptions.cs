namespace SleveenAdmin.Foundation.Options;

public sealed class CaptchaOptions
{
    public const string SectionName = "Captcha";

    public bool Enabled { get; set; } = true;

    public int ExpireMinutes { get; set; } = 5;
}
