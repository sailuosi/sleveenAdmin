using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Security;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.System.Bootstrap.Seed;

namespace SleveenAdmin.System.User.Service;

public sealed class BizSysUserService : BizServiceBase<BizSysUserRow>
{
    private readonly ITenantContext _tenant;

    public BizSysUserService(ITenantContext tenant) => _tenant = tenant;

    public override string BizName => "Biz_Sys_User";

    protected override string TableName => "SysUser";

    protected override string OidColumn => "Su_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => "Su_CreateTime";

    protected override string UpdatedAtColumn => "Su_UpdateTime";

    protected override string DefaultOrderBy => "mtb.Su_CreateTime desc";

    protected override void WhenQueryWhere(SQLBuilder kit, BizQueryInput input)
    {
        base.WhenQueryWhere(kit, input);
        if (!_tenant.IsSuperAdmin && _tenant.TenantId is { } tid)
            kit.whereGuid("mtb.Su_TenantId", tid);
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Su_Account", "mtb.Su_RealName"], kw);
    }

    protected override List<BizSysUserRow> MaterializeList(DataTable dt)
    {
        var list = base.MaterializeList(dt);
        foreach (var row in list)
            row.Password = string.Empty;
        return list;
    }

    protected override BizSysUserRow MaterializeRow(DataRow row)
    {
        var entity = base.MaterializeRow(row);
        entity.Password = string.Empty;
        return entity;
    }

    protected override void NormalizeWriteMap(Dictionary<string, object?> map, BizSysUserRow row)
    {
        base.NormalizeWriteMap(map, row);
        if (string.IsNullOrWhiteSpace(row.PasswordInput))
            map.Remove("Su_Password");
    }

    protected override void OnBeforeSave(BizSysUserRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Account))
            throw new ArgumentException("账号不能为空");
        row.Account = row.Account.Trim();

        var kit = Db.useSQL().from(TableName).where("Su_Account", row.Account);
        if (row.Id != Guid.Empty)
            kit.whereFormat("Su_Id <> {0}", row.Id);
        if (kit.count() > 0)
            throw new InvalidOperationException("账号已存在");

        var exists = row.Id != Guid.Empty && LoadRowById(row.Id) is not null;
        if (!exists)
        {
            if (string.IsNullOrWhiteSpace(row.PasswordInput))
                throw new ArgumentException("密码不能为空");
            row.Password = PasswordHasher.Hash(row.PasswordInput);
        }
        else if (!string.IsNullOrWhiteSpace(row.PasswordInput))
        {
            row.Password = PasswordHasher.Hash(row.PasswordInput);
        }

        if (_tenant.IsSuperAdmin)
            return;

        row.TenantId = _tenant.TenantId;
    }

    protected override void OnBeforeDelete(Guid id)
    {
        if (id == SeedIds.SuperAdminUserId)
            throw new InvalidOperationException("不能删除超级管理员");

        if (!_tenant.IsSuperAdmin)
        {
            var row = LoadRowById(id);
            if (row is null || row.TenantId != _tenant.TenantId)
                throw new InvalidOperationException("无权操作该用户");
        }
    }
}
