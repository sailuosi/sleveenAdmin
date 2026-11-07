namespace SleveenAdmin.Foundation.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "sleveen-admin";

    public string Audience { get; set; } = "sleveen-admin";

    public string SigningKey { get; set; } = "sleveen-admin-dev-signing-key-change-in-production-32bytes!!";

    public int AccessTokenMinutes { get; set; } = 120;

    public int RefreshTokenDays { get; set; } = 7;
}
