using System.Data;
using System.Text.Json;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Foundation.Biz;

/// <summary>
/// 业务 Biz 服务基类（见 doc/biz-module-paradigm.md）。
/// 子类提供表名 / OID 列，并可选覆盖筛选、初始化码表与保存钩子。
/// </summary>
public abstract class BizServiceBase : IScoped
{
    public abstract string BizName { get; }

    protected abstract string TableName { get; }

    protected abstract string OidColumn { get; }

    /// <summary>对外暴露主键列名（Controller 删除入参解析等）。</summary>
    public string PrimaryOidColumn => OidColumn;

    /// <summary>对外暴露主键别名（如 spId），供删除入参解析。</summary>
    public string? PrimaryOidAlias => OidAlias;

    protected virtual string SoftDeleteColumn => "SYS_Deleted";

    /// <summary>创建时间列；门户等表可覆盖为 Sp*_CreateTime。</summary>
    protected virtual string CreatedAtColumn => "SYS_Created";

    /// <summary>更新时间列；门户等表可覆盖为 Sp*_UpdateTime。</summary>
    protected virtual string UpdatedAtColumn => "SYS_LAST_UPD";

    /// <summary>前端主键别名（如 spcId）；空表示不使用。</summary>
    protected virtual string? OidAlias => null;

    protected virtual string DefaultOrderBy => $"mtb.{OidColumn} desc";

    protected DBInstance Db => DBCash.GetDBInstance(MooSqlSetup.DefaultPosition);

    public virtual object GetInitData()
    {
        var dicts = LoadInitDicts();
        return new Dictionary<string, object?>
        {
            ["bizName"] = BizName,
            ["dicts"] = dicts,
            // gene ListVC / EditVC 约定：page.codeTable
            ["codeTable"] = dicts
        };
    }

    protected virtual Dictionary<string, object> LoadInitDicts() => new();

    public virtual object LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*").from($"{TableName} as mtb");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? DefaultOrderBy : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        var records = dt.Rows.Cast<DataRow>().Select(RowToDictionary).ToList();
        return PagedResult<Dictionary<string, object?>>.Create(records, page, size, total);
    }

    public virtual Dictionary<string, object?>? LoadRowById(Guid id)
    {
        var kit = Db.useSQL();
        kit.select("mtb.*").from(TableName + " as mtb")
            .whereGuid("mtb." + OidColumn, id);
        WhenQueryWhere(kit, new BizQueryInput());
        var row = kit.queryRow();
        return row is null ? null : RowToDictionary(row);
    }

    public virtual int SaveList(JsonElement rows)
    {
        if (rows.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("save 入参须为数组");

        var affected = 0;
        foreach (var el in rows.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.Object) continue;
            var map = JsonElementToMap(el);
            OnBeforeSave(map);
            affected += UpsertRow(map);
            OnAfterSave(map);
        }

        return affected;
    }

    public virtual int RemoveByOIDs(IEnumerable<Guid> ids)
    {
        var n = 0;
        foreach (var id in ids)
        {
            if (id == Guid.Empty) continue;
            OnBeforeDelete(id);
            var kit = Db.useSQL();
            kit.setTable(TableName)
                .set(SoftDeleteColumn, 1)
                .set(UpdatedAtColumn, DateTime.Now)
                .whereGuid(OidColumn, id)
                .doUpdate();
            n++;
            OnAfterDelete(id);
        }

        return n;
    }

    protected virtual void WhenQueryWhere(SQLBuilder kit, BizQueryInput input)
    {
        // SoftDeleteColumn 由子类常量定义，非外部输入；可空相等用 whereIsOrNull
        kit.whereIsOrNull("mtb." + SoftDeleteColumn, 0);
        ApplyFilters(kit, input);
        ApplyKeyword(kit, input);
    }

    protected virtual void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            // Filters 的 key 来自请求：必须过标识符白名单，禁止拼进 SQL
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    protected virtual void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        // 子类按需覆盖：模糊搜索用 whereLike / whereLikes，禁止 like '%{kw}%' 字符串拼接
    }

    /// <summary>列名/表别名片段白名单：仅字母数字下划线。</summary>
    protected static bool IsSafeSqlIdent(string? ident)
    {
        if (string.IsNullOrWhiteSpace(ident)) return false;
        for (var i = 0; i < ident.Length; i++)
        {
            var c = ident[i];
            if (c is >= 'a' and <= 'z') continue;
            if (c is >= 'A' and <= 'Z') continue;
            if (c is >= '0' and <= '9') continue;
            if (c == '_') continue;
            return false;
        }

        return ident[0] is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';
    }

    protected virtual void OnBeforeSave(Dictionary<string, object?> map)
    {
        // 前端主键别名 → 真实列
        if (!string.IsNullOrEmpty(OidAlias) &&
            map.TryGetValue(OidAlias, out var aliasOid) &&
            !map.ContainsKey(OidColumn))
        {
            map[OidColumn] = aliasOid;
        }

        // 审计别名 → 真实列（若前端带回）
        CopyIfMissing(map, "SYS_Created", CreatedAtColumn);
        CopyIfMissing(map, "SYS_LAST_UPD", UpdatedAtColumn);
        CopyIfMissing(map, "SYS_Deleted", SoftDeleteColumn);

        // 去掉不可写入的别名键，避免 set 到不存在的列
        StripAliasKeys(map);
    }

    protected virtual void OnAfterSave(Dictionary<string, object?> map) { }

    protected virtual void OnBeforeDelete(Guid id) { }

    protected virtual void OnAfterDelete(Guid id) { }

    /// <summary>输出行字典时可追加前端别名；默认原样列名。</summary>
    protected virtual Dictionary<string, object?> RowToDictionary(DataRow row)
    {
        var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (DataColumn col in row.Table.Columns)
        {
            var v = row[col];
            map[col.ColumnName] = v == DBNull.Value ? null : v;
        }

        ApplyOutputAliases(map);
        return map;
    }

    /// <summary>子类可覆盖，把 Spc_Id→spcId、Spc_CreateTime→SYS_Created 等写入 map（保留原列）。</summary>
    protected virtual void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        if (!string.IsNullOrEmpty(OidAlias) && map.TryGetValue(OidColumn, out var oid))
            map[OidAlias] = oid;
        if (!string.Equals(CreatedAtColumn, "SYS_Created", StringComparison.OrdinalIgnoreCase) &&
            map.TryGetValue(CreatedAtColumn, out var c))
            map["SYS_Created"] = c;
        if (!string.Equals(UpdatedAtColumn, "SYS_LAST_UPD", StringComparison.OrdinalIgnoreCase) &&
            map.TryGetValue(UpdatedAtColumn, out var u))
            map["SYS_LAST_UPD"] = u;
        if (!string.Equals(SoftDeleteColumn, "SYS_Deleted", StringComparison.OrdinalIgnoreCase) &&
            map.TryGetValue(SoftDeleteColumn, out var d))
            map["SYS_Deleted"] = d;
    }

    /// <summary>子类可追加需剥离的前端别名键。</summary>
    protected virtual IEnumerable<string> AliasKeysToStrip()
    {
        if (!string.IsNullOrEmpty(OidAlias))
            yield return OidAlias;
        if (!string.Equals(CreatedAtColumn, "SYS_Created", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_Created";
        if (!string.Equals(UpdatedAtColumn, "SYS_LAST_UPD", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_LAST_UPD";
        if (!string.Equals(SoftDeleteColumn, "SYS_Deleted", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_Deleted";
    }

    void StripAliasKeys(Dictionary<string, object?> map)
    {
        foreach (var k in AliasKeysToStrip())
            map.Remove(k);
    }

    static void CopyIfMissing(Dictionary<string, object?> map, string from, string to)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return;
        if (map.TryGetValue(from, out var v) && !map.ContainsKey(to))
            map[to] = v;
    }

    protected int UpsertRow(Dictionary<string, object?> map)
    {
        var now = DateTime.Now;
        var oid = ResolveOid(map);
        var exists = oid != Guid.Empty &&
                     Db.useSQL().from(TableName)
                         .whereGuid(OidColumn, oid)
                         .whereIsOrNull(SoftDeleteColumn, 0)
                         .count() > 0;

        if (!exists)
        {
            if (oid == Guid.Empty)
                oid = Guid.CreateVersion7();
            // 始终写入本表主键，避免仅有外键 *OID 时漏列
            map[OidColumn] = oid;

            map.TryAdd(CreatedAtColumn, now);
            map[UpdatedAtColumn] = now;
            map.TryAdd(SoftDeleteColumn, 0);

            var kit = Db.useSQL().setTable(TableName);
            foreach (var (k, v) in map)
            {
                if (v is null) continue;
                if (!IsSafeSqlIdent(k)) continue;
                kit.set(k, v);
            }

            return kit.doInsert();
        }

        map[OidColumn] = oid;
        map[UpdatedAtColumn] = now;
        var upd = Db.useSQL().setTable(TableName);
        foreach (var (k, v) in map)
        {
            if (k.Equals(OidColumn, StringComparison.OrdinalIgnoreCase)) continue;
            if (v is null) continue;
            if (!IsSafeSqlIdent(k)) continue;
            upd.set(k, v);
        }

        return upd.whereGuid(OidColumn, oid).doUpdate();
    }

    /// <summary>
    /// 只认本表主键列 <see cref="OidColumn"/>，避免把 En_TopicOID 等外键误当主键。
    /// 无主键时再兼容 OidAlias / id / Id。
    /// </summary>
    protected Guid ResolveOid(Dictionary<string, object?> map)
    {
        if (map.TryGetValue(OidColumn, out var pk) && TryToGuid(pk, out var g))
            return g;

        if (!string.IsNullOrEmpty(OidAlias) &&
            map.TryGetValue(OidAlias, out var alias) && TryToGuid(alias, out g))
            return g;

        if (map.TryGetValue("id", out var id) && TryToGuid(id, out g))
            return g;

        return Guid.Empty;
    }

    protected static bool TryToGuid(object? value, out Guid guid)
    {
        guid = Guid.Empty;
        if (value is null) return false;
        if (value is Guid g)
        {
            guid = g;
            return g != Guid.Empty;
        }

        var s = value.ToString();
        return Guid.TryParse(s, out guid) && guid != Guid.Empty;
    }

    protected static Dictionary<string, object?> JsonElementToMap(JsonElement el)
    {
        var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in el.EnumerateObject())
            map[prop.Name] = JsonValueToObject(prop.Value);
        return map;
    }

    protected static object? JsonValueToObject(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => el.GetString(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number when el.TryGetInt64(out var l) => l,
        JsonValueKind.Number when el.TryGetDouble(out var d) => d,
        JsonValueKind.Array or JsonValueKind.Object => el.GetRawText(),
        _ => el.ToString()
    };

    protected Dictionary<string, List<object>> LoadDictItems(params string[] codes)
    {
        var result = new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
        {
            var items = new List<object>();
            var typeRow = Db.useSQL().select("Sdt_Id").from("SysDictType").where("Sdt_Code", code).queryRow();
            if (typeRow is not null)
            {
                var typeId = typeRow["Sdt_Id"];
                var dt = Db.useSQL().select("Sdd_Value,Sdd_Label,Sdd_OrderNo")
                    .from("SysDictData").where("Sdd_TypeId", typeId)
                    .orderBy("Sdd_OrderNo asc").query();
                foreach (DataRow r in dt.Rows)
                {
                    items.Add(new
                    {
                        value = r["Sdd_Value"]?.ToString(),
                        label = r["Sdd_Label"]?.ToString()
                    });
                }
            }

            result[code] = items;
        }

        return result;
    }
}
