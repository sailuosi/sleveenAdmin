using mooSQL.data;
using mooSQL.data.model;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Officeman.Ebook.DBLayer;

namespace SleveenAdmin.Officeman.Ebook;

public sealed class EbookDbInitializer : IScoped
{
    private static readonly Type[] EntityTypes =
    [
        typeof(Sa_EbCata),
        typeof(Sa_EbMat),
        typeof(Sa_EbTopic),
        typeof(Sa_EbNote),
        typeof(Sa_EbTpMat)
    ];

    private readonly DBInsCash _cash;

    public EbookDbInitializer(DBInsCash cash) => _cash = cash;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var db = _cash.getInstance(MooSqlSetup.DefaultPosition);
        db.useDDL().doInitTable(new DMLOption(), EntityTypes);
        EnsureDicts(db);
        return Task.CompletedTask;
    }

    private static void EnsureDicts(DBInstance db)
    {
        SeedDict(db, EbookSchema.DictPlatform, "素材平台",
        [
            ("xhs", "小红书"), ("wechat", "公众号"), ("bilibili", "B站"), ("douyin", "抖音"),
            ("x", "X"), ("producthunt", "Product Hunt"), ("youtube", "YouTube"), ("weibo", "微博")
        ]);
        SeedDict(db, EbookSchema.DictMaterialStatus, "素材状态",
        [
            ("raw", "待筛选"), ("screened", "已收藏"), ("analyzed", "已拆解")
        ]);
        SeedDict(db, EbookSchema.DictStageGraphic, "图文选题阶段",
        [
            ("inspiration", "灵感"), ("draft", "初稿"), ("final", "定稿"), ("graphic", "图文"), ("publish", "发布")
        ]);
        SeedDict(db, EbookSchema.DictStageVideo, "视频选题阶段",
        [
            ("inspiration", "灵感"), ("outline", "大纲"), ("script", "脚本"),
            ("filming", "拍摄"), ("editing", "剪辑"), ("publish", "发布")
        ]);
        SeedDict(db, EbookSchema.DictPublishFormat, "发布形式",
        [
            ("graphic", "图文"), ("video", "视频"), ("text", "纯文本")
        ]);
        SeedDict(db, EbookSchema.DictTopicStatus, "选题/笔记状态",
        [
            ("draft", "初稿"), ("final", "定稿"), ("published", "已发布")
        ]);
    }

    private static void SeedDict(DBInstance db, string code, string name, (string value, string label)[] items)
    {
        var kit = db.useSQL();
        if (kit.from("SysDictType").where("Sdt_Code", code).count() > 0)
            return;

        var typeId = Guid.CreateVersion7();
        kit.setTable("SysDictType")
            .set("Sdt_Id", typeId)
            .set("Sdt_Code", code)
            .set("Sdt_Name", name)
            .doInsert();

        var idx = 0;
        foreach (var (value, label) in items)
        {
            kit.clear().setTable("SysDictData")
                .set("Sdd_Id", Guid.CreateVersion7())
                .set("Sdd_TypeId", typeId)
                .set("Sdd_Label", label)
                .set("Sdd_Value", value)
                .set("Sdd_OrderNo", ++idx)
                .doInsert();
        }
    }
}
