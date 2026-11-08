using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Foundation.Biz;

/// <summary>
/// 业务 Biz 服务基类（强类型行）。子类提供表名 / OID 列，并可选覆盖筛选、初始化码表与保存钩子。
/// </summary>
public abstract class BizServiceBase<TRow> : IScoped
    where TRow : class, new()
{
    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public abstract string BizName { get; }

    protected abstract string TableName { get; }

    protected abstract string OidColumn { get; }

    /// <summary>对外暴露主键列名（Controller 删除入参解析等）。</summary>
    public string PrimaryOidColumn => OidColumn;

    /// <summary>对外暴露主键别名（如 spId），供删除入参解析。</summary>
    public string? PrimaryOidAlias => OidAlias;

    /// <summary>默认软删；系统域表无 SYS_Deleted 时覆盖为 false。</summary>
    protected virtual bool UseSoftDelete => true;

    protected virtual string SoftDeleteColumn => "SYS_Deleted";

    /// <summary>创建时间列；空字符串表示不写审计列。门户等表可覆盖为 Sp*_CreateTime。</summary>
    protected virtual string CreatedAtColumn => "SYS_Created";

    /// <summary>更新时间列；空字符串表示不写。</summary>
    protected virtual string UpdatedAtColumn => "SYS_LAST_UPD";

    /// <summary>前端主键别名（如 spcId）；空表示不使用。</summary>
    protected virtual string? OidAlias => null;

    protected virtual string DefaultOrderBy => $"mtb.{OidColumn} desc";

    protected DBInstance Db => DBCash.GetDBInstance(MooSqlSetup.DefaultPosition);

    public virtual BizInitData GetInitData()
    {
        var dicts = LoadInitDicts();
        return new BizInitData
        {
            BizName = BizName,
            Dicts = dicts,
            CodeTable = dicts
        };
    }

    protected virtual Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        new(StringComparer.OrdinalIgnoreCase);

    public virtual PagedResult<TRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*").from($"{TableName} as mtb");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? DefaultOrderBy : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        var records = MaterializeList(dt);
        return PagedResult<TRow>.Create(records, page, size, total);
    }

    public virtual TRow? LoadRowById(Guid id)
    {
        var kit = Db.useSQL();
        kit.select("mtb.*").from(TableName + " as mtb")
            .whereGuid("mtb." + OidColumn, id);
        WhenQueryWhere(kit, new BizQueryInput());
        var row = kit.queryRow();
        return row is null ? null : MaterializeRow(row);
    }

    public virtual BizAffectedResult SaveList(IReadOnlyList<TRow> rows)
    {
        var affected = 0;
        foreach (var row in rows)
        {
            if (row is null) continue;
            OnBeforeSave(row);
            affected += UpsertRow(row);
            OnAfterSave(row);
        }

        return BizAffectedResult.Of(affected);
    }

    public virtual BizAffectedResult RemoveByOIDs(IEnumerable<Guid> ids)
    {
        var n = 0;
        foreach (var id in ids)
        {
            if (id == Guid.Empty) continue;
            OnBeforeDelete(id);
            if (UseSoftDelete)
            {
                var kit = Db.useSQL().setTable(TableName)
                    .set(SoftDeleteColumn, 1);
                if (!string.IsNullOrEmpty(UpdatedAtColumn))
                    kit.set(UpdatedAtColumn, DateTime.Now);
                kit.whereGuid(OidColumn, id).doUpdate();
            }
            else
            {
                Db.useSQL().setTable(TableName)
                    .whereGuid(OidColumn, id)
                    .doDelete();
            }

            n++;
            OnAfterDelete(id);
        }

        return BizAffectedResult.Of(n);
    }

    protected virtual void WhenQueryWhere(SQLBuilder kit, BizQueryInput input)
    {
        if (UseSoftDelete)
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
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    protected virtual void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        // 子类按需覆盖：模糊搜索用 whereLike / whereLikes
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

    protected virtual void OnBeforeSave(TRow row) { }

    protected virtual void OnAfterSave(TRow row) { }

    protected virtual void OnBeforeDelete(Guid id) { }

    protected virtual void OnAfterDelete(Guid id) { }

    protected virtual List<TRow> MaterializeList(DataTable dt) => Db.toList<TRow>(dt);

    protected virtual TRow MaterializeRow(DataRow row) => Db.toEntity<TRow>(row);

    /// <summary>将行转为可写入列字典（SooColumn / 属性名）。</summary>
    protected virtual Dictionary<string, object?> RowToWriteMap(TRow row)
    {
        var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in typeof(TRow).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
            var col = ResolveDbColumnName(prop);
            if (col is null) continue;
            if (!IsSafeSqlIdent(col)) continue;
            map[col] = prop.GetValue(row);
        }

        return map;
    }

    /// <summary>仅带 <see cref="SooColumnAttribute"/> 的属性参与写入，避免把计算列 / 联表字段落库。</summary>
    protected virtual string? ResolveDbColumnName(PropertyInfo prop)
    {
        var soo = prop.GetCustomAttribute<SooColumnAttribute>(inherit: true);
        if (soo is null) return null;
        return string.IsNullOrWhiteSpace(soo.Name) ? prop.Name : soo.Name;
    }

    protected int UpsertRow(TRow row)
    {
        var map = RowToWriteMap(row);
        NormalizeWriteMap(map, row);
        var now = DateTime.Now;
        var oid = ResolveOid(map, row);
        var exists = oid != Guid.Empty && RowExists(oid);

        if (!exists)
        {
            if (oid == Guid.Empty)
                oid = Guid.CreateVersion7();
            map[OidColumn] = oid;
            SetOidOnRow(row, oid);

            if (!string.IsNullOrEmpty(CreatedAtColumn))
                map.TryAdd(CreatedAtColumn, now);
            if (!string.IsNullOrEmpty(UpdatedAtColumn))
                map[UpdatedAtColumn] = now;
            if (UseSoftDelete)
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
        if (!string.IsNullOrEmpty(UpdatedAtColumn))
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

    /// <summary>写入前剥离别名键、补主键映射；子类可扩展。</summary>
    protected virtual void NormalizeWriteMap(Dictionary<string, object?> map, TRow row)
    {
        if (!string.IsNullOrEmpty(OidAlias) &&
            map.TryGetValue(OidAlias, out var aliasOid) &&
            !map.ContainsKey(OidColumn))
        {
            map[OidColumn] = aliasOid;
        }

        if (!string.IsNullOrEmpty(CreatedAtColumn))
            CopyIfMissing(map, "SYS_Created", CreatedAtColumn);
        if (!string.IsNullOrEmpty(UpdatedAtColumn))
            CopyIfMissing(map, "SYS_LAST_UPD", UpdatedAtColumn);
        if (UseSoftDelete)
            CopyIfMissing(map, "SYS_Deleted", SoftDeleteColumn);

        foreach (var k in AliasKeysToStrip())
            map.Remove(k);
    }

    protected virtual IEnumerable<string> AliasKeysToStrip()
    {
        if (!string.IsNullOrEmpty(OidAlias))
            yield return OidAlias;
        if (!string.IsNullOrEmpty(CreatedAtColumn) &&
            !string.Equals(CreatedAtColumn, "SYS_Created", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_Created";
        if (!string.IsNullOrEmpty(UpdatedAtColumn) &&
            !string.Equals(UpdatedAtColumn, "SYS_LAST_UPD", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_LAST_UPD";
        if (UseSoftDelete &&
            !string.Equals(SoftDeleteColumn, "SYS_Deleted", StringComparison.OrdinalIgnoreCase))
            yield return "SYS_Deleted";
    }

    static void CopyIfMissing(Dictionary<string, object?> map, string from, string to)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return;
        if (map.TryGetValue(from, out var v) && !map.ContainsKey(to))
            map[to] = v;
    }

    bool RowExists(Guid oid)
    {
        var kit = Db.useSQL().from(TableName).whereGuid(OidColumn, oid);
        if (UseSoftDelete)
            kit.whereIsOrNull(SoftDeleteColumn, 0);
        return kit.count() > 0;
    }

    protected Guid ResolveOid(Dictionary<string, object?> map, TRow row)
    {
        if (map.TryGetValue(OidColumn, out var pk) && TryToGuid(pk, out var g))
            return g;

        if (!string.IsNullOrEmpty(OidAlias) &&
            map.TryGetValue(OidAlias, out var alias) && TryToGuid(alias, out g))
            return g;

        if (map.TryGetValue("id", out var id) && TryToGuid(id, out g))
            return g;

        // 实体属性主键（如 SysUser.Id）
        foreach (var prop in typeof(TRow).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var soo = prop.GetCustomAttribute<SooColumnAttribute>(inherit: true);
            if (soo?.IsPrimaryKey == true && TryToGuid(prop.GetValue(row), out g))
                return g;
        }

        return Guid.Empty;
    }

    protected void SetOidOnRow(TRow row, Guid oid)
    {
        foreach (var prop in typeof(TRow).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;
            var soo = prop.GetCustomAttribute<SooColumnAttribute>(inherit: true);
            if (soo is not null &&
                (soo.IsPrimaryKey ||
                 string.Equals(soo.Name, OidColumn, StringComparison.OrdinalIgnoreCase)))
            {
                if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
                {
                    prop.SetValue(row, oid);
                    return;
                }
            }

            if (string.Equals(prop.Name, OidColumn, StringComparison.OrdinalIgnoreCase) &&
                (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?)))
            {
                prop.SetValue(row, oid);
                return;
            }
        }
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

    protected Dictionary<string, List<BizDictItem>> LoadDictItems(params string[] codes)
    {
        var result = new Dictionary<string, List<BizDictItem>>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
        {
            var items = new List<BizDictItem>();
            var typeRow = Db.useSQL().select("Sdt_Id").from("SysDictType").where("Sdt_Code", code).queryRow();
            if (typeRow is not null)
            {
                var typeId = typeRow["Sdt_Id"];
                var dt = Db.useSQL().select("Sdd_Value,Sdd_Label,Sdd_OrderNo")
                    .from("SysDictData").where("Sdd_TypeId", typeId)
                    .orderBy("Sdd_OrderNo asc").query();
                foreach (DataRow r in dt.Rows)
                {
                    items.Add(new BizDictItem
                    {
                        Value = r["Sdd_Value"]?.ToString(),
                        Label = r["Sdd_Label"]?.ToString()
                    });
                }
            }

            result[code] = items;
        }

        return result;
    }

    /// <summary>从 JSON 反序列化为行列表（Controller 可选桥接）。</summary>
    public static List<TRow> DeserializeRows(JsonElement rows)
    {
        if (rows.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("save 入参须为数组");
        return JsonSerializer.Deserialize<List<TRow>>(rows.GetRawText(), JsonOpts) ?? [];
    }
}
