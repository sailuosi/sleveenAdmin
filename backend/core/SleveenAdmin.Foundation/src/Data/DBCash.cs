using mooSQL.data;
using mooSQL.linq;

namespace SleveenAdmin.Foundation.Data;

/// <summary>
/// mooSQL 静态入口，与 DI 注册的 <see cref="DBInsCash"/> 同源（由 <see cref="MooSqlSetup"/> Bind）。
/// </summary>
public static class DBCash
{
    private static DBInsCash? _cash;

    public static void Bind(DBInsCash cash) => _cash = cash ?? throw new ArgumentNullException(nameof(cash));

    public static DBInsCash RequireCash() =>
        _cash ?? throw new InvalidOperationException("DBCash 未初始化，请先调用 services.AddMooSql(configuration)。");

    public static DBInstance GetDBInstance(int position) => RequireCash().getInstance(position);

    public static DBInstance GetDBInstance(string name) => RequireCash().getInstance(name);

    public static SQLBuilder useSQL(int position) => GetDBInstance(position).useSQL();

    public static SQLBuilder useSQL(string name) => GetDBInstance(name).useSQL();

    public static DbBus<T> useBus<T>(int position)
    {

        var db = GetDBInstance(position);
        return db.useDbBus<T>();
    }

    public static SooRepository<T> useRepo<T>(int position) where T : class, new() =>
        GetDBInstance(position).useRepo<T>();

    public static SooUnitOfWork useUnitOfWork(int position) => GetDBInstance(position).useWork();

    public static SooUnitOfWork useWork(int position) => GetDBInstance(position).useWork();

    public static DDLBuilder useDDL(int position) => new DDLBuilder(GetDBInstance(position));

    public static SQLClip useClip(int position) => GetDBInstance(position).useClip();

    public static IDbQuery<T> useQueryable<T>(int position) where T : class, new() =>
        GetDBInstance(position).useQueryable<T>();
}
