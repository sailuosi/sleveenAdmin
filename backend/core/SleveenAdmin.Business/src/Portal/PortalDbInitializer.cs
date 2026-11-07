using SleveenAdmin.Business.Portal.DBLayer;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using mooSQL.data;
using mooSQL.data.model;

namespace SleveenAdmin.Business.Portal;

public sealed class PortalDbInitializer : IScoped
{
    private static readonly Type[] EntityTypes =
    [
        typeof(SysPortal),
        typeof(SysPortPage),
        typeof(SysPortCell),
        typeof(SysPortCellLink),
        typeof(SysApplet),
        typeof(SysAppletCata),
        typeof(SysPortalRole),
        typeof(SysPortalLink),
        typeof(SysPageSelect),
        typeof(SysPageField),
        typeof(SysPageConnect)
    ];

    private readonly DBInsCash _cash;

    public PortalDbInitializer(DBInsCash cash) => _cash = cash;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var db = _cash.getInstance(MooSqlSetup.DefaultPosition);
        db.useDDL().doInitTable(new DMLOption(), EntityTypes);
        // 门户菜单已并入 MenuSeedCatalog（DbInitializer），此处不再重复插菜单
        EnsurePortalDicts(db);
        EnsureHomePortPage(db);
        return Task.CompletedTask;
    }

    /// <summary>确保存在 PP_Code=home 的门户页面，供首页 loadhomeportal 绑定。</summary>
    private static void EnsureHomePortPage(DBInstance db)
    {
        var kit = db.useSQL();
        if (kit.from(PortalSchema.TablePortPage).where("PP_Code", "home").where("(Spp_Deleted is null or Spp_Deleted=0)").count() > 0)
            return;

        // 避开菜单 Id 080（流程引擎）
        kit.clear().setTable(PortalSchema.TablePortPage)
            .set("Spp_Id", Guid.Parse("00000000-0000-0000-0000-000000000075"))
            .set("Spp_CreateTime", DateTime.Now)
            .set("Spp_UpdateTime", DateTime.Now)
            .set("Spp_Deleted", false)
            .set("PP_Title", "首页")
            .set("PP_Code", "home")
            .set("PP_IsOn", true)
            .set("PP_Idx", 0)
            .doInsert();
    }

    private static void EnsurePortalDicts(DBInstance db)
    {
        SeedDict(db, "SysPtTask", "门户业务", [("1", "通用"), ("2", "办公"), ("3", "生产")]);
        SeedDict(db, "SysPtCellTheme", "门户主题", [("default", "默认"), ("dark", "深色"), ("light", "浅色")]);
        SeedDict(db, "SysShareStatus", "共享状态", [("1", "公共"), ("2", "个人"), ("0", "私有")]);
        SeedDict(db, "SysAppletTitle", "显示标题", [("1", "显示"), ("0", "隐藏")]);
        SeedDict(db, "SysPtDisplay", "布局方式", [("grid", "网格"), ("flex", "弹性")]);
        SeedDict(db, "SysAppletType", "组件类型", [("1", "Vue组件"), ("2", "URL"), ("3", "HTML"), ("5", "图表"), ("7", "子门户")]);
    }

    private static void SeedDict(DBInstance db, string code, string name, (string value, string label)[] items)
    {
        var typeId = Guid.CreateVersion7();
        var kit = db.useSQL();
        if (kit.from(PortalSchema.TableSysDictType).where("Sdt_Code", code).count() > 0)
            return;
        kit.setTable(PortalSchema.TableSysDictType)
            .set("Sdt_Id", typeId)
            .set("Sdt_Code", code)
            .set("Sdt_Name", name)
            .doInsert();
        var idx = 0;
        foreach (var (value, label) in items)
        {
            kit.clear().setTable(PortalSchema.TableSysDictData)
                .set("Sdd_Id", Guid.CreateVersion7())
                .set("Sdd_TypeId", typeId)
                .set("Sdd_Label", label)
                .set("Sdd_Value", value)
                .set("Sdd_OrderNo", ++idx)
                .doInsert();
        }
    }
}
