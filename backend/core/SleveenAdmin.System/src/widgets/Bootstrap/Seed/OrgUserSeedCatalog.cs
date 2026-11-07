using SleveenAdmin.System.Bootstrap.Constants;
using SleveenAdmin.System.Org.DBLayer;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.Bootstrap.Seed;

/// <summary>
/// 演示组织/人员种子：2 层组织（XX科技公司 → 开发部/客服部）及部门人员。
/// </summary>
public static class OrgUserSeedCatalog
{
    public const string DefaultPassword = "123456";
    public const string CompanyName = "XX科技公司";
    public const string CompanyCode = "XX_TECH";
    public const string CompanyClassCode = "001";

    public static SysOrg CreateCompany(DateTime createTime) => new()
    {
        Id = SeedIds.RootOrgId,
        Pid = null,
        Name = CompanyName,
        Code = CompanyCode,
        ClassCode = CompanyClassCode,
        OrderNo = 1,
        CreateTime = createTime
    };

    public static IReadOnlyList<SysOrg> CreateDepartments(DateTime createTime) =>
    [
        new SysOrg
        {
            Id = SeedIds.OrgDevDeptId,
            Pid = SeedIds.RootOrgId,
            Name = "XX开发部",
            Code = "XX_DEV",
            ClassCode = "001001",
            OrderNo = 1,
            CreateTime = createTime
        },
        new SysOrg
        {
            Id = SeedIds.OrgCsDeptId,
            Pid = SeedIds.RootOrgId,
            Name = "XX客服部",
            Code = "XX_CS",
            ClassCode = "001002",
            OrderNo = 2,
            CreateTime = createTime
        }
    ];

    /// <summary>不含超级管理员；密码需调用方 Hash。</summary>
    public static IReadOnlyList<(Guid Id, string Account, string RealName, Guid OrgId)> DemoUsers { get; } =
    [
        // 开发部 3 人
        (SeedIds.UserZhangSanId, "zhangsan", "张三", SeedIds.OrgDevDeptId),
        (SeedIds.UserLiSiId, "lisi", "李四", SeedIds.OrgDevDeptId),
        (SeedIds.UserWangWuId, "wangwu", "王五", SeedIds.OrgDevDeptId),
        // 客服部 5 人
        (SeedIds.UserZhaoLiuId, "zhaoliu", "赵六", SeedIds.OrgCsDeptId),
        (SeedIds.UserQianQiId, "qianqi", "钱七", SeedIds.OrgCsDeptId),
        (SeedIds.UserSunBaId, "sunba", "孙八", SeedIds.OrgCsDeptId),
        (SeedIds.UserZhouJiuId, "zhoujiu", "周九", SeedIds.OrgCsDeptId),
        (SeedIds.UserWuShiId, "wushi", "吴十", SeedIds.OrgCsDeptId)
    ];

    public static SysUser CreateDemoUser(
        Guid id,
        string account,
        string realName,
        Guid orgId,
        string passwordHash,
        DateTime createTime) => new()
    {
        Id = id,
        Account = account,
        Password = passwordHash,
        RealName = realName,
        Status = UserStatus.Enabled,
        OrgId = orgId,
        TenantId = SeedIds.DefaultTenantId,
        CreateTime = createTime
    };
}
