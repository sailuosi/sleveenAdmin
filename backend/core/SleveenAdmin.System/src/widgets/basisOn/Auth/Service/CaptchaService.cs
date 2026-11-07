using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Options;
using SleveenAdmin.System.Auth.Dtos;

namespace SleveenAdmin.System.Auth.Service;

public sealed class CaptchaService : ISingleton
{
    private const string CachePrefix = "captcha:";
    private const string Chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    private readonly IMemoryCache _cache;
    private readonly CaptchaOptions _options;

    public CaptchaService(IMemoryCache cache, IOptions<CaptchaOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public CaptchaResult Create()
    {
        var code = new char[4];
        for (var i = 0; i < code.Length; i++)
            code[i] = Chars[Random.Shared.Next(Chars.Length)];

        var codeText = new string(code);
        var captchaId = Guid.CreateVersion7().ToString("N");
        _cache.Set(CachePrefix + captchaId, codeText, TimeSpan.FromMinutes(_options.ExpireMinutes));

        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="120" height="40">
            <rect width="100%" height="100%" fill="#f4f4f5"/>
            <text x="50%" y="54%" dominant-baseline="middle" text-anchor="middle"
                  font-size="22" fill="#333" font-family="Consolas,monospace">{codeText}</text>
            </svg>
            """;

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return new CaptchaResult
        {
            CaptchaId = captchaId,
            CaptchaImage = $"data:image/svg+xml;base64,{base64}"
        };
    }

    public bool Validate(string? captchaId, string? captchaCode)
    {
        if (!_options.Enabled)
            return true;

        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(captchaCode))
            return false;

        var key = CachePrefix + captchaId.Trim();
        if (!_cache.TryGetValue(key, out string? stored))
            return false;

        _cache.Remove(key);
        return string.Equals(stored, captchaCode.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
