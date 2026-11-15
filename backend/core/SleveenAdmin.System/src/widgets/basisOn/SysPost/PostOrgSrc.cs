namespace SleveenAdmin.System.SysPost;

/// <summary>岗位组织来源代码（对齐 minehub HH_DOrgSrc）。</summary>
public static class PostOrgSrc
{
    /// <summary>行政组织 → HR_Organize。</summary>
    public const string AdminOrg = "1";

    /// <summary>位置 → SysOrg。</summary>
    public const string Place = "2";

    public static string Label(string? src) => src switch
    {
        AdminOrg => "行政组织",
        Place => "位置",
        _ => src ?? ""
    };
}
