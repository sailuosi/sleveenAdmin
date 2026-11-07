using mooSQL.auth;

namespace SleveenAdmin.System.Author.AuthBag.Word;

public sealed class DutyWordBag : WordBagDialect
{
    public DutyWordBag(AuthDialect root) : base(root) { }

    public void useMapToHR()
    {
        foreach (var group in groups.Values)
        {
            if (group is DutyWordGroup g)
                g.MapToHROrg();
        }
    }

    protected override bool onCheckEmpty()
    {
        foreach (var group in groups.Values)
        {
            if (group is DutyWordGroup g && !g.isEmptyMyCustom())
                return false;
        }

        return true;
    }

    private DutyWordGroup? getDutyGroup(string group = "_empty")
    {
        var t = getGroup(group);
        return t as DutyWordGroup;
    }

    public void addBindOrgs(List<AuthOrg> orgs)
    {
        foreach (var org in orgs) addBindOrg(org);
    }

    public void addContainOrgs(List<AuthOrg> orgs)
    {
        foreach (var org in orgs) addContainOrg(org);
    }

    public int addBindHROrg(AuthOrg org, string group = "_empty")
    {
        var g = getDutyGroup(group);
        return g?.HROrg.addBindValue(org) == true ? 1 : 0;
    }

    public int addBindHROrgs(List<AuthOrg> orgs, string group = "_empty")
    {
        var g = getDutyGroup(group);
        if (g == null) return 0;
        foreach (var org in orgs) g.HROrg.addBindValue(org);
        return orgs.Count;
    }

    public int addContainHROrg(AuthOrg org, string group = "_empty")
    {
        var g = getDutyGroup(group);
        return g?.HROrg.addContainValue(org) == true ? 1 : 0;
    }

    public int addContainHROrgs(List<AuthOrg> orgs, string group = "_empty")
    {
        var g = getDutyGroup(group);
        if (g == null) return 0;
        foreach (var org in orgs) g.HROrg.addContainValue(org);
        return orgs.Count;
    }

    public DutyWordBag forEachGroup(Action<DutyWordGroup> action)
    {
        foreach (var g in groups)
        {
            if (g.Value is DutyWordGroup r) action(r);
        }

        return this;
    }
}
