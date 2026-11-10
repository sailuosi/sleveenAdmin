using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Security;
using SleveenAdmin.System.Bootstrap.Constants;
using SleveenAdmin.System.Bootstrap.Seed;
using SleveenAdmin.System.AuthBag.DBLayer;
using SleveenAdmin.System.Auth.DBLayer;
using SleveenAdmin.System.Config.DBLayer;
using SleveenAdmin.System.Dict.DBLayer;
using SleveenAdmin.System.File.DBLayer;
using SleveenAdmin.System.Menu.DBLayer;
using SleveenAdmin.System.Notice.DBLayer;
using SleveenAdmin.System.Org.DBLayer;
using SleveenAdmin.System.Tenant.DBLayer;
using SleveenAdmin.System.User.DBLayer;
using mooSQL.data;
using mooSQL.data.model;

namespace SleveenAdmin.System.Bootstrap;

[Scoped]
public sealed class DbInitializer : IDbInitializer
{
    private static readonly Type[] EntityTypes =
    [
        typeof(SysUser),
        typeof(SysMenu),
        typeof(SysOrg),
        typeof(SysDictType),
        typeof(SysDictData),
        typeof(SysConfig),
        typeof(SysFile),
        typeof(SysRefreshToken),
        typeof(SysTenant),
        typeof(SysNotice),
        typeof(SysNoticeUser),
        typeof(AuthDutyItem),
        typeof(AuthGoodsBag),
        typeof(AuthGoodsItem),
        typeof(AuthVisitorBag),
        typeof(AuthVisitorItem),
        typeof(AuthWordBag),
        typeof(AuthWordItem),
        typeof(AuthWordDefine)
    ];

    private readonly DBInsCash _cash;

    public DbInitializer(DBInsCash cash) => _cash = cash;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var db = _cash.getInstance(MooSqlSetup.DefaultPosition);
        var executor = new DBExecutor(db);
        executor.ExeQueryScalar<int>(new SQLCmd(db.dialect.sentence.getPingSQL(), new Paras()));

        db.useDDL().doInitTable(new DMLOption(), EntityTypes);
        SeedIfEmpty(db);
        EnsureOrgAndUsersFromCatalog(db);
        EnsureMenusFromCatalog(db);
        EnsureDefaultTenant(db);
        return Task.CompletedTask;
    }

    private static void SeedIfEmpty(DBInstance db)
    {
        var userRepo = db.useRepo<SysUser>();
        if (userRepo.Count(_ => true) > 0)
            return;

        var now = DateTime.Now;
        var admin = new SysUser
        {
            Id = SeedIds.SuperAdminUserId,
            Account = "admin",
            Password = PasswordHasher.Hash(OrgUserSeedCatalog.DefaultPassword),
            RealName = "超级管理员",
            Status = UserStatus.Enabled,
            OrgId = SeedIds.RootOrgId,
            TenantId = SeedIds.DefaultTenantId,
            CreateTime = now
        };

        var work = db.useWork();
        work.Insert(admin);
        work.Insert(OrgUserSeedCatalog.CreateCompany(now));
        work.Commit();
    }

    /// <summary>
    /// 幂等补种演示组织与人员：公司（根）+ 开发部/客服部 + 部门人员（不覆盖已有行）。
    /// 已有「根组织」会同步改名为 XX科技公司。
    /// </summary>
    private static void EnsureOrgAndUsersFromCatalog(DBInstance db)
    {
        var orgRepo = db.useRepo<SysOrg>();
        var userRepo = db.useRepo<SysUser>();
        var now = DateTime.Now;
        var work = db.useWork();
        var dirty = false;

        var root = orgRepo.GetById(SeedIds.RootOrgId);
        if (root is null)
        {
            work.Insert(OrgUserSeedCatalog.CreateCompany(now));
            dirty = true;
        }
        else if (root.Name is "根组织" or "ROOT" ||
                 string.IsNullOrWhiteSpace(root.ClassCode))
        {
            root.Name = OrgUserSeedCatalog.CompanyName;
            root.Code = OrgUserSeedCatalog.CompanyCode;
            root.ClassCode = OrgUserSeedCatalog.CompanyClassCode;
            orgRepo.Update(root);
        }

        foreach (var dept in OrgUserSeedCatalog.CreateDepartments(now))
        {
            if (orgRepo.Exist(dept.Id))
                continue;
            work.Insert(dept);
            dirty = true;
        }

        foreach (var (id, account, realName, orgId) in OrgUserSeedCatalog.DemoUsers)
        {
            if (userRepo.Exist(id))
                continue;
            work.Insert(OrgUserSeedCatalog.CreateDemoUser(
                id,
                account,
                realName,
                orgId,
                PasswordHasher.Hash(OrgUserSeedCatalog.DefaultPassword),
                now));
            dirty = true;
        }

        if (dirty)
            work.Commit();
    }

    /// <summary>按 <see cref="MenuSeedCatalog"/> 幂等补种缺失菜单（不覆盖已有行；父先子后）。</summary>
    private static void EnsureMenusFromCatalog(DBInstance db)
    {
        var menuRepo = db.useRepo<SysMenu>();
        var missing = MenuSeedCatalog.All.Where(m => !menuRepo.Exist(m.Id)).ToList();
        if (missing.Count == 0)
            return;

        var byId = missing.ToDictionary(m => m.Id);
        var ordered = new List<SysMenu>(missing.Count);
        var visiting = new HashSet<Guid>();

        void Visit(SysMenu menu)
        {
            if (!visiting.Add(menu.Id))
                return;
            if (menu.Pid is Guid pid && byId.TryGetValue(pid, out var parent))
                Visit(parent);
            ordered.Add(menu);
        }

        foreach (var menu in missing)
            Visit(menu);

        var work = db.useWork();
        foreach (var menu in ordered)
            work.Insert(menu);
        work.Commit();
    }

    private static void EnsureDefaultTenant(DBInstance db)
    {
        var tenantRepo = db.useRepo<SysTenant>();
        if (tenantRepo.Exist(SeedIds.DefaultTenantId))
            return;

        tenantRepo.Insert(new SysTenant
        {
            Id = SeedIds.DefaultTenantId,
            Name = "默认租户",
            Code = "default",
            Status = 1,
            Remark = "系统默认租户",
            CreateTime = DateTime.Now
        });
    }
}
