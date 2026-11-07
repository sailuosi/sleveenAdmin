using mooSQL.auth;

namespace SleveenAdmin.System.Author.AuthBag;

public sealed class DutyFactory : AuthFactory<DutyLoader>
{
    public override DutyLoader GetDialect() => new();
}
