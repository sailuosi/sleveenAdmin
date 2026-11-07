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
        EnsureEbookHomeApplets(db);
        EnsureEbookHomePortal(db);
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

    /// <summary>
    /// 门户码表种子（对齐 minehub PortalDictSeedData，编码沿用前端已用的 Sys*/KB_*）。
    /// 幂等：类型已存在则复用 Id，仅补缺失的字典项。
    /// </summary>
    private static void EnsurePortalDicts(DBInstance db)
    {
        // minehub: HH_AppletMany / HH_AppletType / HH_ApplteTool / HH_PtCellTheme / HH_PtDisplay / HH_ShareStatus / HH_AppletTitle
        SeedDict(db, "SysAppletMany", "门户多播",
            [("1", "tab"), ("2", "瀑布流"), ("3", "轮播"), ("4", "卡片")]);
        SeedDict(db, "SysAppletType", "门户控件类型",
        [
            ("1", "vue组件"), ("11", "APP组件"), ("2", "url"), ("3", "富文本"),
            ("4", "本地页面BPO"), ("5", "echart"), ("6", "本地VC"), ("7", "子门户"), ("9", "伪组件")
        ]);
        SeedDict(db, "SysApplteTool", "门户工具栏",
            [("1", "刷新"), ("2", "更多"), ("3", "最大化")]);
        SeedDict(db, "SysPtCellTheme", "门户主题",
            [("0", "无"), ("1", "白色"), ("2", "蓝色"), ("3", "透明")]);
        SeedDict(db, "SysPtDisplay", "门户布局方式",
            [("1", "grid布局"), ("2", "弹性布局"), ("3", "table布局"), ("9", "开发者布局")]);
        SeedDict(db, "SysShareStatus", "门户共享类型",
            [("1", "共享"), ("2", "个人")]);
        SeedDict(db, "SysAppletTitle", "控件标题位置",
            [("0", "隐藏"), ("1", "顶部"), ("2", "左侧"), ("3", "右侧"), ("4", "底部")]);

        // sleveen 前端额外用到、minehub 门户种子未覆盖的码表
        SeedDict(db, "SysPtTask", "门户业务",
            [("1", "通用"), ("2", "办公"), ("3", "生产")]);
        SeedDict(db, "SysCellDisabled", "可用性",
            [("0", "可用"), ("1", "禁用")]);
        SeedDict(db, "KB_PtPagePose", "页面位置",
            [("1", "顶部"), ("2", "左侧"), ("3", "右侧"), ("4", "底部")]);
        SeedDict(db, "KB_HOPtLinkPlace", "链接位置",
            [("1", "顶部"), ("2", "侧边"), ("3", "底部")]);
        SeedDict(db, "KB_Wether", "是否",
            [("1", "是"), ("0", "否")]);
    }

    private static void SeedDict(DBInstance db, string code, string name, (string value, string label)[] items)
    {
        var kit = db.useSQL();
        Guid typeId;
        var exist = kit.clear()
            .select("Sdt_Id")
            .from(PortalSchema.TableSysDictType)
            .where("Sdt_Code", code)
            .queryRow();
        if (exist is null)
        {
            typeId = Guid.CreateVersion7();
            kit.clear().setTable(PortalSchema.TableSysDictType)
                .set("Sdt_Id", typeId)
                .set("Sdt_Code", code)
                .set("Sdt_Name", name)
                .doInsert();
        }
        else
        {
            typeId = exist["Sdt_Id"] is Guid g ? g : Guid.Parse(exist["Sdt_Id"]!.ToString()!);
        }

        var idx = 0;
        foreach (var (value, label) in items)
        {
            idx++;
            var has = kit.clear()
                .from(PortalSchema.TableSysDictData)
                .where("Sdd_TypeId", typeId)
                .where("Sdd_Value", value)
                .count() > 0;
            if (has) continue;

            kit.clear().setTable(PortalSchema.TableSysDictData)
                .set("Sdd_Id", Guid.CreateVersion7())
                .set("Sdd_TypeId", typeId)
                .set("Sdd_Label", label)
                .set("Sdd_Value", value)
                .set("Sdd_OrderNo", idx)
                .doInsert();
        }
    }

    /// <summary>创作工作台首页 5 个 Vue Portlet 控件。</summary>
    private static void EnsureEbookHomeApplets(DBInstance db)
    {
        var now = DateTime.Now;
        var applets = new (Guid id, string name, string key, int idx)[]
        {
            (Guid.Parse("00000000-0000-0000-0000-0000000000B1"), "创作欢迎栏", "officeman/views/ebook/home/vcEbWelcome", 1),
            (Guid.Parse("00000000-0000-0000-0000-0000000000B2"), "创作统计栏", "officeman/views/ebook/home/vcEbStatBar", 2),
            (Guid.Parse("00000000-0000-0000-0000-0000000000B3"), "创作产出趋势", "officeman/views/ebook/home/vcEbOutputTrend", 3),
            (Guid.Parse("00000000-0000-0000-0000-0000000000B4"), "互动影响", "officeman/views/ebook/home/vcEbEngageImpact", 4),
            (Guid.Parse("00000000-0000-0000-0000-0000000000B5"), "创作健康", "officeman/views/ebook/home/vcEbHealth", 5),
        };

        foreach (var (id, name, key, idx) in applets)
        {
            var kit = db.useSQL();
            if (kit.from(PortalSchema.TableApplet).where("A_Key", key).where("(Sa_Deleted is null or Sa_Deleted=0)").count() > 0)
                continue;

            kit.clear().setTable(PortalSchema.TableApplet)
                .set("Sa_Id", id)
                .set("Sa_CreateTime", now)
                .set("Sa_UpdateTime", now)
                .set("Sa_Deleted", false)
                .set("A_IsOn", true)
                .set("A_Name", name)
                .set("A_Key", key)
                .set("A_Type", "1")
                .set("A_Title", name)
                .set("A_Idx", idx)
                .set("A_Remark", "创作工作台首页门户组件")
                .doInsert();
        }
    }

    /// <summary>确保 P_Key=home 门户存在并挂上创作工作台默认格子（仅在尚无格子时写入）。</summary>
    private static void EnsureEbookHomePortal(DBInstance db)
    {
        var kit = db.useSQL();
        var pageId = Guid.Parse("00000000-0000-0000-0000-000000000075");
        var portalId = Guid.Parse("00000000-0000-0000-0000-0000000000B0");
        var now = DateTime.Now;

        if (kit.from(PortalSchema.TablePortal).where("P_Key", "home").where("(Sp_Deleted is null or Sp_Deleted=0)").count() == 0)
        {
            kit.clear().setTable(PortalSchema.TablePortal)
                .set("Sp_Id", portalId)
                .set("Sp_CreateTime", now)
                .set("Sp_UpdateTime", now)
                .set("Sp_Deleted", false)
                .set("Sp_PortPageId", pageId)
                .set("P_IsOn", true)
                .set("P_Name", "创作工作台首页")
                .set("P_Key", "home")
                .set("P_ColNum", "12")
                .set("P_Rows", "40")
                .set("P_Display", "grid")
                .set("P_Pulic", "1")
                .set("P_ShowTitle", "0")
                .set("P_Idx", 0)
                .set("P_Editable", true)
                .doInsert();
        }
        else
        {
            var row = kit.clear().select("Sp_Id as oid").from(PortalSchema.TablePortal)
                .where("P_Key", "home")
                .where("(Sp_Deleted is null or Sp_Deleted=0)")
                .orderBy("P_Idx asc")
                .queryRow();
            if (row != null && Guid.TryParse(row["oid"]?.ToString(), out var existing))
                portalId = existing;
        }

        if (kit.clear().from(PortalSchema.TablePortCell).where("Spc_PortalId", portalId).where("(Spc_Deleted is null or Spc_Deleted=0)").count() > 0)
            return;

        var cells = new (Guid id, string name, string key, string x, string y, string w, string h, int idx)[]
        {
            (Guid.Parse("00000000-0000-0000-0000-0000000000C1"), "欢迎栏", "eb-welcome", "0", "0", "12", "8", 1),
            (Guid.Parse("00000000-0000-0000-0000-0000000000C2"), "统计栏", "eb-stat", "0", "8", "12", "3", 2),
            (Guid.Parse("00000000-0000-0000-0000-0000000000C3"), "产出趋势", "eb-trend", "0", "11", "4", "8", 3),
            (Guid.Parse("00000000-0000-0000-0000-0000000000C4"), "互动影响", "eb-engage", "4", "11", "4", "8", 4),
            (Guid.Parse("00000000-0000-0000-0000-0000000000C5"), "创作健康", "eb-health", "8", "11", "4", "8", 5),
        };

        var appletKeys = new Dictionary<string, (Guid appletId, string path, string title)>
        {
            ["eb-welcome"] = (Guid.Parse("00000000-0000-0000-0000-0000000000B1"), "officeman/views/ebook/home/vcEbWelcome", "创作欢迎栏"),
            ["eb-stat"] = (Guid.Parse("00000000-0000-0000-0000-0000000000B2"), "officeman/views/ebook/home/vcEbStatBar", "创作统计栏"),
            ["eb-trend"] = (Guid.Parse("00000000-0000-0000-0000-0000000000B3"), "officeman/views/ebook/home/vcEbOutputTrend", "创作产出趋势"),
            ["eb-engage"] = (Guid.Parse("00000000-0000-0000-0000-0000000000B4"), "officeman/views/ebook/home/vcEbEngageImpact", "互动影响"),
            ["eb-health"] = (Guid.Parse("00000000-0000-0000-0000-0000000000B5"), "officeman/views/ebook/home/vcEbHealth", "创作健康"),
        };

        foreach (var (id, name, key, x, y, w, h, idx) in cells)
        {
            kit.clear().setTable(PortalSchema.TablePortCell)
                .set("Spc_Id", id)
                .set("Spc_CreateTime", now)
                .set("Spc_UpdateTime", now)
                .set("Spc_Deleted", false)
                .set("Spc_PortalId", portalId)
                .set("PC_Idx", idx)
                .set("PC_IsOn", true)
                .set("PC_Name", name)
                .set("PC_X", x)
                .set("PC_Y", y)
                .set("PC_W", w)
                .set("PC_H", h)
                .set("PC_I", idx.ToString())
                .set("PC_Key", key)
                .set("PC_Many", "0")
                .doInsert();

            var (appletId, path, title) = appletKeys[key];
            kit.clear().setTable(PortalSchema.TablePortCellLink)
                .set("Spcl_Id", Guid.Parse($"00000000-0000-0000-0000-0000000000D{idx}"))
                .set("Spcl_CreateTime", now)
                .set("Spcl_UpdateTime", now)
                .set("Spcl_Deleted", false)
                .set("Spcl_PortalId", portalId)
                .set("Spcl_PortCellId", id)
                .set("Spcl_AppletId", appletId)
                .set("CL_Link", key)
                .set("CL_Key", path)
                .set("CL_Title", title)
                .set("CL_Type", "1")
                .set("CL_Idx", 0)
                .set("CL_IsOn", true)
                .doInsert();
        }
    }
}
