using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.Options;
using SleveenAdmin.Foundation.Security;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.System.Auth.AppRelease;
using SleveenAdmin.System.Auth.DBLayer;
using SleveenAdmin.System.Auth.Dtos;
using SleveenAdmin.System.Author.AuthBag;
using SleveenAdmin.System.Bootstrap.Constants;
using SleveenAdmin.System.Menu.DBLayer;
using SleveenAdmin.System.User.DBLayer;
using mooSQL.data;

namespace SleveenAdmin.System.Auth.Service;

[Scoped]
public sealed class AuthService {
    private readonly MooRepository<SysUser> _users;
    private readonly MooRepository<SysMenu> _menus;
    private readonly MooRepository<SysRefreshToken> _refreshTokens;
    private readonly CaptchaService _captchaService;
    private readonly JwtOptions _jwt;

    public AuthService(
        MooRepository<SysUser> users,
        MooRepository<SysMenu> menus,
        MooRepository<SysRefreshToken> refreshTokens,
        CaptchaService captchaService,
        IOptions<JwtOptions> jwt)
    {
        _users = users;
        _menus = menus;
        _refreshTokens = refreshTokens;
        _captchaService = captchaService;
        _jwt = jwt.Value;
    }

    public Task<LoginResult?> LoginAsync(LoginInput input, CancellationToken cancellationToken = default)
    {
        if (!_captchaService.Validate(input.CaptchaId, input.CaptchaCode))
            throw new InvalidOperationException("验证码错误或已过期");

        var user = _users.Repo.GetList(u => u.Account == input.UserName).FirstOrDefault();
        if (user is null || user.Status != UserStatus.Enabled)
            return Task.FromResult<LoginResult?>(null);

        if (!PasswordHasher.Verify(input.Password, user.Password))
            return Task.FromResult<LoginResult?>(null);

        return Task.FromResult<LoginResult?>(CreateTokens(user));
    }

    public Task<UserInfoResult?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = _users.Repo.GetById(userId);
        if (user is null)
            return Task.FromResult<UserInfoResult?>(null);

        var buttons = GetUserButtons(user);
        var release = AppReleaseCatalog.Current;

        return Task.FromResult<UserInfoResult?>(new UserInfoResult
        {
            UserId = user.Id.ToString(),
            Account = user.Account,
            UserName = user.RealName ?? user.Account,
            Roles = [],
            Buttons = buttons,
            AppVersion = release.Version,
            UpdateNotes = release.UpdateNotes,
            UpdateTip = release.UpdateTip
        });
    }

    public LoginResult RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new InvalidOperationException("RefreshToken 不能为空");

        var hash = RefreshTokenHasher.Hash(refreshToken.Trim());
        var stored = _refreshTokens.Repo.GetFirst(t =>
            t.TokenHash == hash && !t.IsRevoked && t.ExpireTime > DateTime.Now);

        if (stored is null)
            throw new InvalidOperationException("RefreshToken 无效或已过期");

        var user = _users.Repo.GetById(stored.UserId)
                   ?? throw new InvalidOperationException("用户不存在");

        stored.IsRevoked = true;
        _refreshTokens.Repo.Update(stored);

        return CreateTokens(user);
    }

    private List<string> GetUserButtons(SysUser user)
    {
        if (string.Equals(user.Account, AuthRoleCodes.AdminAccount, StringComparison.OrdinalIgnoreCase))
        {
            return _menus.Repo.GetList(m =>
                    m.Permission != null &&
                    (m.Type == MenuTypes.Button || m.Type == MenuTypes.Menu))
                .Select(m => m.Permission!)
                .Distinct()
                .ToList();
        }

        var ctx = ToAuthContext(user, isSuperAdmin: false);
        var allowed = new List<string>();
        DBCash.useSQL(MooSqlSetup.DefaultPosition).useDutyGoods(ctx, "1", ids => allowed.AddRange(ids));
        if (allowed.Count == 0)
            return [];

        var menuIds = allowed
            .Select(s => Guid.TryParse(s, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        return _menus.Repo.GetList(m =>
                menuIds.Contains(m.Id) &&
                m.Permission != null &&
                (m.Type == MenuTypes.Button || m.Type == MenuTypes.Menu))
            .Select(m => m.Permission!)
            .Distinct()
            .ToList();
    }

    private LoginResult CreateTokens(SysUser user)
    {
        var permissions = GetUserButtons(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Account),
            new(ClaimTypes.Name, user.Account),
            new("uid", user.Id.ToString())
        };

        if (user.TenantId is not null)
            claims.Add(new Claim(TenantClaimTypes.TenantId, user.TenantId.Value.ToString()));

        if (user.OrgId is not null)
            claims.Add(new Claim("orgId", user.OrgId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(user.RealName))
            claims.Add(new Claim("realName", user.RealName));

        foreach (var permission in permissions)
        {
            if (permission.HasText())
                claims.Add(new Claim(PermissionClaimTypes.Permission, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        var refreshToken = Guid.CreateVersion7().ToString("N");
        var now = DateTime.Now;
        _refreshTokens.Repo.Insert(new SysRefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = RefreshTokenHasher.Hash(refreshToken),
            ExpireTime = now.AddDays(_jwt.RefreshTokenDays),
            IsRevoked = false,
            CreateTime = now
        });

        return new LoginResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken
        };
    }

    private static AuthUserContext ToAuthContext(SysUser user, bool isSuperAdmin) => new()
    {
        UserId = user.Id,
        Account = user.Account,
        RealName = user.RealName,
        OrgId = user.OrgId,
        TenantId = user.TenantId,
        IsSuperAdmin = isSuperAdmin
    };
}
