using mooSQL.auth;
using mooSQL.data;
using mooSQL.utils;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.System.Author.AuthBag.Word;

namespace SleveenAdmin.System.Author.AuthBag;

public partial class DutyBuilder : AuthorBuilder<DutyLoader>
{
    public IAuthUserContext? user;
    public VisitorFilter visitFilter;
    private bool adminAll;
    private bool _useLoginVisitorBag = true;
    private bool useMapToHRCalled;

    public DutyWordBag wordBag => (DutyWordBag)dialect.wordBag;

    public DutyBuilder()
    {
        factory = new DutyFactory();
        visitFilter = new VisitorFilter();
    }

    public AuthUser CastToUser(IAuthUserContext manager)
    {
        if (manager.IsSuperAdmin)
        {
            return new AuthUser
            {
                Acount = manager.Account,
                Name = manager.RealName,
                dialect = factory.GetDialect()
            };
        }

        return dialect.getUser(manager.Account);
    }

    public DutyBuilder useMapToHR()
    {
        useMapToHRCalled = true;
        return this;
    }

    public DutyBuilder usePemisson(string pemissionCode, string group = "3")
    {
        goodsBag.addGoods(new AuthGoods { code = pemissionCode, group = group });
        return this;
    }

    public DutyBuilder useAdminAll(bool isAdminAllData = true)
    {
        adminAll = isAdminAllData;
        return this;
    }

    public DutyBuilder useMenu(long menuId, string group = "1")
    {
        goodsBag.addGoods(new AuthGoods { id = menuId, group = group, type = "1", code = menuId.ToString() });
        return this;
    }

    public DutyBuilder useMenu(string pageCode, string group = "2")
    {
        goodsBag.addGoods(new AuthGoods { code = pageCode, group = group, type = "1" });
        return this;
    }

    public DutyBuilder setUser(IAuthUserContext loginUser)
    {
        user = loginUser;
        return this;
    }

    public override void readRoleDataScope()
    {
        if (user is null) return;
        if (adminAll && user.IsSuperAdmin)
        {
            wordBag.addAll();
            return;
        }

        var au = CastToUser(user);
        foreach (var roleScope in dataScopes)
            readRole(roleScope, au);

        if (useMapToHRCalled)
            wordBag.useMapToHR();
    }

    private int generalBagCode()
    {
        var hash = new HashCode();
        if (goodsBag is { Goods.Count: > 0 })
        {
            foreach (var goo in goodsBag.Goods)
                hash.Add(string.Concat(goo.oid, goo.code, goo.type));
        }

        if (visitFilter is { Empty: false })
            hash.Add(visitFilter.loadBags());

        if (user != null) hash.Add(user.Account);
        return hash.ToHashCode();
    }

    public override List<AuthWord> loadDataScopes()
    {
        if (user is null) return [];
        if (_useLoginVisitorBag && adminAll && user.IsSuperAdmin)
        {
            wordBag.addAll();
            return [];
        }

        var key = "loadRole:" + generalBagCode();
        var cached = AuthorServices.Cache.Get<List<AuthWord>>(key);
        if (cached != null) return cached;

        var roles = dialect.loadRole(user, kit =>
        {
            if (_useLoginVisitorBag)
            {
                var myDutys = DutyLoader.loadDutysByWorkerAndVisitor(user);
                if (myDutys.Count > 0)
                    kit.whereIn("d.Adi_Id", myDutys);
            }

            if (visitFilter is { Empty: false })
            {
                var bags = visitFilter.loadBags();
                if (bags.Count > 0)
                    kit.whereIn("d.Adi_VisitorBagId", bags);
            }

            if (goodsBag is { Goods.Count: > 0 })
            {
                kit.whereIn("d.Adi_GoodsBagId", e =>
                {
                    e.select("g.Agi_GoodsBagId").from("AuthGoodsItem g");
                    foreach (var goods in goodsBag.Goods)
                    {
                        if (goods.type == "1" && !string.IsNullOrWhiteSpace(goods.code))
                        {
                            e.sink().where("g.Agi_Code", goods.code).where("Agi_Type", goods.type).rise();
                        }
                        else if (goods.type == "1" && !string.IsNullOrWhiteSpace(goods.oid))
                        {
                            e.sink().where("g.Agi_Oid", goods.oid).where("Agi_Type", goods.type).rise();
                        }
                        else if (!string.IsNullOrWhiteSpace(goods.code))
                        {
                            e.sink().where("g.Agi_Code", goods.code).where("Agi_Type", goods.type).rise();
                        }
                        else if (!string.IsNullOrWhiteSpace(goods.oid))
                        {
                            e.sink().where("g.Agi_Oid", goods.oid).where("Agi_Type", goods.type).rise();
                        }
                    }
                });
            }

            _loadRoles?.Invoke(kit);
        });

        AuthorServices.Cache.Set(key, roles, TimeSpan.FromMinutes(20));
        return roles;
    }

    public DutyBuilder useLoginVisitBag(bool isUseLoginManVisitorBag)
    {
        _useLoginVisitorBag = isUseLoginManVisitorBag;
        return this;
    }

    public DutyBuilder useManVisit(string manCode)
    {
        visitFilter.addMan(manCode);
        return this;
    }

    public DutyBuilder useOrgVisit(string orgOid)
    {
        visitFilter.addOrg(orgOid);
        return this;
    }

    public DutyBuilder onBagIsEmpty(Action<VisitorFilter> action)
    {
        visitFilter.onBagIsEmpty(action);
        return this;
    }

    public DutyBuilder useHROrgIsField(string fieldName)
    {
        whereHROrgIs(org =>
        {
            kit?.where(fieldName, org.HROID);
            return $"{fieldName}='{org.HROID}'";
        }).whereHROrgIn(orgs =>
        {
            if (orgs.Count == 0) return null;
            var uids = orgs.map(m => m.HROID);
            kit?.whereIn(fieldName, uids);
            return $"{fieldName} in ('{string.Join("','", uids)}')";
        }).whereHROrgLikeByFK(org =>
        {
            if (kit != null && org.ClassCode.HasText())
            {
                kit.whereIn(fieldName, g =>
                {
                    g.select("ogzpk.So_Id")
                        .from("SysOrg ogzpk")
                        .whereLikeLeft("ogzpk.So_ClassCode", org.ClassCode);
                });
            }

            return string.Empty;
        });
        return this;
    }

    public DutyBuilder whereHROrgIs(Func<AuthOrg, string> doBudgetIs)
    {
        wordBag.forEachGroup(g => g.HROrg.useIsBuilder(doBudgetIs));
        return this;
    }

    public DutyBuilder whereHROrgLikeByFK(Func<AuthOrg, string> doBudgetIs)
    {
        wordBag.forEachGroup(g =>
            g.HROrg.useOneBuilder((org, islike) => islike ? doBudgetIs(org) : string.Empty));
        return this;
    }

    public DutyBuilder whereHROrgIn(Func<List<AuthOrg>, string?> doBudgetIs)
    {
        wordBag.forEachGroup(g => g.HROrg.useInBuilder(doBudgetIs));
        return this;
    }
}
