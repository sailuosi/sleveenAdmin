using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.System.Notice.Constants;
namespace SleveenAdmin.System.Notice.Service;

public sealed class BizSysNoticeService : BizServiceBase<BizSysNoticeRow>
{
    private readonly ITenantContext _tenant;

    public BizSysNoticeService(ITenantContext tenant) => _tenant = tenant;

    public override string BizName => "Biz_Sys_Notice";

    protected override string TableName => "SysNotice";

    protected override string OidColumn => "Sn_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => "Sn_CreateTime";

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.Sn_CreateTime desc";

    protected override void WhenQueryWhere(SQLBuilder kit, BizQueryInput input)
    {
        base.WhenQueryWhere(kit, input);
        if (!_tenant.IsSuperAdmin && _tenant.TenantId is { } tid)
            kit.whereGuid("mtb.Sn_TenantId", tid);
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLike("mtb.Sn_Title", input.Keyword.Trim());
    }

    protected override void OnBeforeSave(BizSysNoticeRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Title))
            throw new ArgumentException("标题不能为空");
        row.Title = row.Title.Trim();
        row.Content ??= string.Empty;

        var exists = row.Id != Guid.Empty && LoadRowById(row.Id) is not null;
        if (!exists)
        {
            var userId = _tenant.UserId ?? throw new InvalidOperationException("未登录");
            row.CreateUserId = userId;
            row.Status = NoticeStatus.Draft;
            row.TenantId = _tenant.IsSuperAdmin ? row.TenantId : _tenant.TenantId;
        }
        else
        {
            EnsureTenantAccess(row.TenantId);
        }
    }

    protected override void OnBeforeDelete(Guid id)
    {
        var row = LoadRowById(id) ?? throw new InvalidOperationException("通知不存在");
        EnsureTenantAccess(row.TenantId);
        Db.useSQL().setTable("SysNoticeUser").whereGuid("Snu_NoticeId", id).doDelete();
    }

    public void Publish(Guid id)
    {
        var notice = LoadRowById(id) ?? throw new InvalidOperationException("通知不存在");
        EnsureTenantAccess(notice.TenantId);

        Db.useSQL().setTable(TableName)
            .set("Sn_Status", NoticeStatus.Published)
            .set("Sn_PublishTime", DateTime.Now)
            .whereGuid(OidColumn, id)
            .doUpdate();

        if (notice.Type == NoticeTypes.Announcement)
            EnsureAnnouncementReceivers(id, notice.TenantId);
    }

    private void EnsureTenantAccess(Guid? tenantId)
    {
        if (_tenant.IsSuperAdmin)
            return;

        if (tenantId != _tenant.TenantId)
            throw new InvalidOperationException("无权操作该租户数据");
    }

    private void EnsureAnnouncementReceivers(Guid noticeId, Guid? tenantId)
    {
        var userKit = Db.useSQL().select("Su_Id").from("SysUser");
        if (!_tenant.IsSuperAdmin && tenantId is { } tid)
            userKit.whereGuid("Su_TenantId", tid);
        var dt = userKit.query();

        foreach (DataRow ur in dt.Rows)
        {
            if (ur["Su_Id"] is not Guid userId || userId == Guid.Empty)
                continue;

            var exists = Db.useSQL().from("SysNoticeUser")
                .whereGuid("Snu_NoticeId", noticeId)
                .whereGuid("Snu_UserId", userId)
                .count() > 0;
            if (exists) continue;

            Db.useSQL().setTable("SysNoticeUser")
                .set("Snu_Id", Guid.CreateVersion7())
                .set("Snu_NoticeId", noticeId)
                .set("Snu_UserId", userId)
                .set("Snu_IsRead", false)
                .doInsert();
        }
    }
}
