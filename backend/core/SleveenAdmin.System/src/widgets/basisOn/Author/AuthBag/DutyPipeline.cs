using System.Text.Json;
using mooSQL.auth;

namespace SleveenAdmin.System.Author.AuthBag;

public sealed class DutyPipeline : PipelineDialect
{
    public DutyLoader dialect;

    public DutyPipeline(DutyLoader dialect) => this.dialect = dialect;

    public Word.DutyWordBag? wordBag =>
        dialect.wordBag is Word.DutyWordBag r ? r : null;

    public override int readRoleScopeCode(WordBagDialect range, AuthWord role, AuthUser user)
    {
        switch (role.scopeCode)
        {
            case "4":
                range.addAll();
                break;
            case "3":
                range.addContainOrg(user.HisDivision);
                break;
            case "2":
                range.addBindOrg(user.HisDivision);
                break;
            case "1":
                range.addMan(user);
                break;
            case "7":
                range.addContainOrg(user.HisOrg);
                break;
            case "6":
                range.addBindOrg(user.HisOrg);
                break;
            default:
                if (role.type == "2")
                    range.addLazyWord(role);
                else if (role.type == "3")
                    range.addLiveWord(role);
                else if (role.scopeCode == "t01")
                    range.addLazyWord(role);
                break;
        }

        return 0;
    }

    public override ConditionGroup? parseAuthWord(AuthWord role)
    {
        if (string.IsNullOrWhiteSpace(role.parser)) return null;
        try
        {
            return JsonSerializer.Deserialize<ConditionGroup>(role.parser);
        }
        catch
        {
            return null;
        }
    }
}
