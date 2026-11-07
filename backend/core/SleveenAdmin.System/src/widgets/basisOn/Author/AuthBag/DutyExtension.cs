using mooSQL.auth;
using mooSQL.data;
using mooSQL.utils;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.System.Author.AuthBag;

public static class DutyExtension
{
    public static SQLBuilder useDuty(this SQLBuilder kit, IAuthUserContext user, Action<DutyBuilder> buildAuth)
    {
        var tar = new DutyBuilder();
        tar.useAdminAll();
        tar.setUser(user);
        tar.useSQLBuilder(kit);
        buildAuth(tar);
        return kit;
    }

    public static SQLBuilder useDuty(this SQLBuilder kit, DutyBuilder auth, Action<DutyBuilder> buildAuth)
    {
        buildAuth(auth);
        return kit;
    }

    public static SQLBuilder useDutyGoods(this SQLBuilder kit, IAuthUserContext user, string goodsType,
        Action<List<string>> filter)
    {
        var myVisitBag = DutyLoader.loadMyVisitorBag(user);
        if (myVisitBag.Count > 0)
        {
            var ckit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
            var menuIds = ckit.select("g.Agi_Oid")
                .from("AuthGoodsItem g join AuthDutyItem d on g.Agi_GoodsBagId=d.Adi_GoodsBagId")
                .whereIn("d.Adi_VisitorBagId", myVisitBag)
                .where("g.Agi_Type", goodsType)
                .where("(d.Adi_IsDeleted=0 or d.Adi_IsDeleted is null)")
                .query();
            filter(menuIds.getFieldValues("Agi_Oid"));
            return kit;
        }

        filter(myVisitBag);
        return kit;
    }

    public static bool filterDutyGoods(IAuthUserContext user, string goodsOid, string goodsType)
    {
        var myVisitBag = DutyLoader.loadMyVisitorBag(user);
        if (myVisitBag.Count == 0) return false;

        var ckit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        return ckit.select("g.Agi_Oid")
                   .from("AuthGoodsItem g join AuthDutyItem d on g.Agi_GoodsBagId=d.Adi_GoodsBagId")
                   .whereIn("d.Adi_VisitorBagId", myVisitBag)
                   .where("g.Agi_Oid", goodsOid)
                   .where("g.Agi_Type", goodsType)
                   .count() > 0;
    }
}
