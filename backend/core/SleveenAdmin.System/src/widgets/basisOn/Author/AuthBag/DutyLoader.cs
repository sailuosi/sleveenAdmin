using System.Text.Json;
using mooSQL.auth;
using mooSQL.data;
using mooSQL.utils;
using mooSQL.utils;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.System.Author.AuthBag.Word;

namespace SleveenAdmin.System.Author.AuthBag;

public sealed class DutyLoader : AuthDialect
{
    public DutyLoader()
    {
        pipeline = new DutyPipeline(this);
        wordBag = new DutyWordBag(this);
    }

    public DutyPipeline pipeline;
    public DutyWordBag wordBag;

    public override PipelineDialect getPipeLine() => pipeline;
    public override WordBagDialect getWordBag() => wordBag;
    public override WordGroupBag newWordGroupBag() => new DutyWordGroup();

    public override AuthOrg getOrgByOID(string oid)
    {
        var row = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("So_Id,So_Code,So_ClassCode,So_Name")
            .from("SysOrg")
            .where("So_Id", oid)
            .queryRow();
        return row == null ? null! : MapOrg(row);
    }

    public override List<AuthOrg> getOrgByOIDs(List<string> oid) =>
        DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("So_Id,So_Code,So_ClassCode,So_Name")
            .from("SysOrg")
            .whereIn("So_Id", oid)
            .query(MapOrg);

    public List<AuthWord> loadRole(IAuthUserContext user, Action<SQLBuilder> onloading)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        onloading(kit);
        return kit.select("f.Awd_Name,f.Awd_Code,f.Awd_Type,f.Awd_Para,f.Awd_Parser,w.Awi_Id,d.Adi_Id")
            .from("AuthWordItem w join AuthWordDefine f on w.Awi_WordDefineId=f.Awd_Id")
            .join("join AuthDutyItem d on w.Awi_WordBagId=d.Adi_WordBagId")
            .where("(d.Adi_IsDeleted=0 or d.Adi_IsDeleted is null)")
            .query(row => new AuthWord
            {
                id = row["Awi_Id"].ToString()!,
                scopeCode = row.getString("Awd_Code", ""),
                code = row.getString("Awd_Type", ""),
                name = row.getString("Awd_Name", ""),
                para = row.getString("Awd_Para", ""),
                parser = row.getString("Awd_Parser", ""),
                type = row.getString("Awd_Type", ""),
                roleId = row["Adi_Id"].ToString()!
            });
    }

    public static List<string> loadMyVisitorBag(IAuthUserContext user, bool useCache = true)
    {
        var key = "userVisitorBags:" + user.Account;
        if (useCache)
        {
            var cached = AuthorServices.Cache.Get<List<string>>(key);
            if (cached != null) return cached;
        }

        var orgId = user.OrgId?.ToString() ?? string.Empty;
        var code = loadOrgCode(orgId);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var bagsDt = kit.select("v.Avi_VisitorBagId")
            .from("AuthVisitorItem v")
            .where("(v.Avi_IsOn=1 or v.Avi_IsOn is null)")
            .whereFormat(
                "(v.Avi_Type='1' and v.Avi_Code ={0}) or (v.Avi_Type='2' and v.Avi_Oid ={1}) or (v.Avi_Type='3' and {2} like concat(v.Avi_Code,'%'))",
                user.Account, orgId, code ?? string.Empty)
            .query();

        var tar = bagsDt.getFieldValues("Avi_VisitorBagId");
        if (useCache) AuthorServices.Cache.Set(key, tar);
        return tar;
    }

    public static List<string> loadHisVisitorBag(string manCode, bool useCache = true)
    {
        var key = "userVisitorBags:" + manCode;
        if (useCache)
        {
            var cached = AuthorServices.Cache.Get<List<string>>(key);
            if (cached != null) return cached;
        }

        var manDiv = loadManDivision(manCode);
        var code = loadOrgCode(manDiv);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var bagsDt = kit.select("v.Avi_VisitorBagId")
            .from("AuthVisitorItem v")
            .where("(v.Avi_IsOn=1 or v.Avi_IsOn is null)")
            .whereFormat(
                "(v.Avi_Type='1' and v.Avi_Code ={0}) or (v.Avi_Type='2' and v.Avi_Oid ={1}) or (v.Avi_Type='3' and {2} like concat(v.Avi_Code,'%'))",
                manCode, manDiv, code ?? string.Empty)
            .query();

        var tar = bagsDt.getFieldValues("Avi_VisitorBagId");
        if (useCache) AuthorServices.Cache.Set(key, tar);
        return tar;
    }

    public static string? loadOrgCode(string? orgOid)
    {
        if (string.IsNullOrWhiteSpace(orgOid) || !Guid.TryParse(orgOid, out _))
            return null;

        var key = "orgCode:" + orgOid;
        var cached = AuthorServices.Cache.Get<string>(key);
        if (!string.IsNullOrWhiteSpace(cached)) return cached;

        var val = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("o.So_ClassCode,o.So_Code")
            .from("SysOrg o")
            .where("o.So_Id", orgOid)
            .queryRowString("");

        if (string.IsNullOrWhiteSpace(val))
        {
            val = DBCash.useSQL(MooSqlSetup.DefaultPosition)
                .select("o.So_Code")
                .from("SysOrg o")
                .where("o.So_Id", orgOid)
                .queryRowString("");
        }

        if (!string.IsNullOrWhiteSpace(val))
            AuthorServices.Cache.Set(key, val);

        return val;
    }

    public static string loadManDivision(string manCode)
    {
        var key = "manDivision:" + manCode;
        var cached = AuthorServices.Cache.Get<string>(key);
        if (!string.IsNullOrWhiteSpace(cached)) return cached;

        var val = DBCash.useSQL(MooSqlSetup.DefaultPosition)
            .select("u.Su_OrgId")
            .from("SysUser u")
            .where("u.Su_Account", manCode)
            .queryRowString("");

        if (!string.IsNullOrWhiteSpace(val))
            AuthorServices.Cache.Set(key, val);

        return val;
    }

    public override List<AuthWord> getDefaultRole() => [];

    public override AuthOrg loadManOrg(AuthUser man) => getOrg(k => k.where("o.So_Id", man.orgOID));
    public override AuthOrg loadManDiv(AuthUser man) => getOrg(k => k.where("o.So_Id", man.divisionOID));
    public override AuthPost loadManPost(AuthUser man) => new AuthPost { postNo = man.postNo };

    public override AuthOrg getOrg(Action<SQLBuilder> whereBuilder)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        whereBuilder(kit);
        return kit.select("So_Id,So_Pid,So_Code,So_ClassCode,So_Name")
            .from("SysOrg o")
            .queryRow(MapOrg)!;
    }

    public override AuthPost getPost(Action<SQLBuilder> _) => new AuthPost();

    public override AuthUser getUser(string account) => getUser(k => k.where("u.Su_Account", account));

    public override AuthUser getUser(Action<SQLBuilder> whereBuilder)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        whereBuilder(kit);
        return kit.select("Su_Id,Su_Account,Su_RealName,Su_OrgId")
            .from("SysUser u")
            .queryRow(row => new AuthUser
            {
                UserOID = row["Su_Id"].ToString()!,
                Acount = row["Su_Account"].ToString()!,
                Name = row.getString("Su_RealName", ""),
                divisionOID = row.getString("Su_OrgId", ""),
                orgOID = row.getString("Su_OrgId", ""),
                dialect = this
            })!;
    }

    public static List<string> loadOrgVisitorBag(string orgOid)
    {
        var key = "orgVisitorBags:" + orgOid;
        var cached = AuthorServices.Cache.Get<List<string>>(key);
        if (cached != null) return cached;

        var code = loadOrgCode(orgOid);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var bagsDt = kit.select("v.Avi_VisitorBagId")
            .from("AuthVisitorItem v")
            .where("(v.Avi_IsOn=1 or v.Avi_IsOn is null)")
            .whereFormat("(v.Avi_Type='2' and v.Avi_Oid ={0}) or (v.Avi_Type='3' and {1} like concat(v.Avi_Code,'%'))",
                orgOid, code ?? string.Empty)
            .query();

        var tar = bagsDt.getFieldValues("Avi_VisitorBagId");
        AuthorServices.Cache.Set(key, tar);
        return tar;
    }

    public static List<string> loadDutysByWorkerAndVisitor(IAuthUserContext user)
    {
        var cacheKey = $"userDutys:{user.Account}";
        var cached = AuthorServices.Cache.Get<List<string>>(cacheKey);
        if (cached != null) return cached;

        var res = new List<string>();
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var defaults = kit.select("Adi_Id")
            .from("AuthDutyItem")
            .where("(Adi_IsDeleted=0 or Adi_IsDeleted is null)")
            .where("Adi_Code", "sys_default")
            .queryFirstField<string>()
            .ToList();
        foreach (var r in defaults) res.AddNotRepeat(r);

        var visitBags = loadHisVisitorBag(user.Account, true);
        if (visitBags.Count > 0)
        {
            var dutyItems = kit.clear()
                .select("d.Adi_Id")
                .from("AuthDutyItem d")
                .whereIn("d.Adi_VisitorBagId", visitBags)
                .where("(d.Adi_IsDeleted=0 or d.Adi_IsDeleted is null)")
                .queryFirstField<string>()
                .ToList();
            res.AddNotRepeat(dutyItems);
        }

        AuthorServices.Cache.Set(cacheKey, res);
        return res;
    }

    private static AuthOrg MapOrg(global::System.Data.DataRow row) => new()
    {
        HROID = row["So_Id"].ToString()!,
        OrgNo = row.getString("So_Code", ""),
        Name = row.getString("So_Name", ""),
        HRCode = row.getString("So_ClassCode", row.getString("So_Code", "")),
        ClassCode = row.getString("So_ClassCode", row.getString("So_Code", ""))
    };
}
