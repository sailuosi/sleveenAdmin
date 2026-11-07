namespace SleveenAdmin.System.Author.AuthBag;

public sealed class VisitorFilter
{
    public List<string> mans = [];
    public List<string> orgs = [];
    public Action<VisitorFilter>? whenEmpty;

    public bool Empty => mans.Count == 0 && orgs.Count == 0;

    public void addMan(string man)
    {
        if (!mans.Contains(man)) mans.Add(man);
    }

    public void addOrg(string orgOid)
    {
        if (!orgs.Contains(orgOid)) orgs.Add(orgOid);
    }

    public void onBagIsEmpty(Action<VisitorFilter> action) => whenEmpty = action;

    public List<string> loadBags()
    {
        var res = new List<string>();
        foreach (var org in orgs)
        {
            foreach (var fk in DutyLoader.loadOrgVisitorBag(org))
            {
                if (!res.Contains(fk)) res.Add(fk);
            }
        }

        foreach (var man in mans)
        {
            foreach (var fk in DutyLoader.loadHisVisitorBag(man))
            {
                if (!res.Contains(fk)) res.Add(fk);
            }
        }

        if (res.Count == 0 && whenEmpty != null)
            whenEmpty(this);

        return res;
    }
}
