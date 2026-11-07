namespace SleveenAdmin.System.Auth.Dtos;

public sealed class UserInfoResult
{
    public string UserId { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public IReadOnlyList<string> Buttons { get; set; } = [];

    /// <summary>当前前端发版版本（与 AppReleaseCatalog.Current.Version 一致）</summary>
    public string? AppVersion { get; set; }

    public string? UpdateNotes { get; set; }

    public string? UpdateTip { get; set; }
}
