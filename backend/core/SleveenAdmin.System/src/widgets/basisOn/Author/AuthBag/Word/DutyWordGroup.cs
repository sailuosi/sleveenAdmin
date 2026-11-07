using mooSQL.auth;
using mooSQL.data;

namespace SleveenAdmin.System.Author.AuthBag.Word;

public sealed class DutyWordGroup : WordGroupBag
{
    public CodeRange<AuthOrg> HROrg = new();
    public CodeRange<AuthOrg> accountOrg = new();
    public ItemRange<AuthHROrg> account = new();

    public override List<string> buildWhere(List<string> wh, SQLBuilder kit)
    {
        wh = base.buildWhere(wh, kit);
        wh = HROrg.buildWhere(wh);
        return wh;
    }

    public bool isEmptyMyCustom() => HROrg.Empty;

    public void MapToHROrg()
    {
        // sleveenAdmin 无党建-人力映射表，组织范围直接沿用 SysOrg。
    }
}
