using System.Text.RegularExpressions;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>分类/表单编码：PUBLIC → prefix_slug；UNIT → prefix_u_{orgNo}_slug。</summary>
public static class SailResourceCodes
{
    public static string NormalizeSegment(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var s = raw.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9_-]+", "_");
        s = Regex.Replace(s, @"_+", "_").Trim('_');
        return s.Length > 48 ? s[..48] : s;
    }

    public static string SlugFromName(string? name)
    {
        var slug = NormalizeSegment(name);
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    public static string Build(string prefix, string? authType, string? orgNo, string? nameOrSlug)
    {
        var slug = SlugFromName(nameOrSlug);
        if (SailAuthScope.EqualsType(authType, SailAuthScope.Unit))
        {
            var no = NormalizeSegment(orgNo);
            if (string.IsNullOrEmpty(no)) no = "unit";
            return $"{prefix}_u_{no}_{slug}";
        }
        return $"{prefix}_{slug}";
    }

    /// <summary>优先 HR_Organize.O_OrgNo，否则 SysOrg.Code。</summary>
    public static string? ResolveOrgNo(string? unitId, int masterDbPosition = MooSqlSetup.DefaultPosition)
    {
        if (string.IsNullOrWhiteSpace(unitId)) return null;
        try
        {
            var kit = DBCash.useSQL(masterDbPosition);
            var hr = kit.clear()
                .setTable("HR_Organize")
                .select("O_OrgNo")
                .where("HR_OrganizeOID", unitId.Trim())
                .query();
            if (hr.Rows.Count > 0)
            {
                var no = hr.Rows[0]["O_OrgNo"]?.ToString();
                if (!string.IsNullOrWhiteSpace(no)) return no.Trim();
            }

            var sys = kit.clear()
                .setTable("SysOrg")
                .select("Code")
                .where("Id", unitId.Trim())
                .query();
            if (sys.Rows.Count > 0)
            {
                var code = sys.Rows[0]["Code"]?.ToString();
                if (!string.IsNullOrWhiteSpace(code)) return code.Trim();
            }
        }
        catch
        {
            /* ignore */
        }
        return null;
    }
}
